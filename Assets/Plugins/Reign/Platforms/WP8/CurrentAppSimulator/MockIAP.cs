#if REIGN_POSTBUILD
/* 
    Copyright (c) 2012 Microsoft Corporation.  All rights reserved.
    Use of this sample source code is subject to the terms of the Microsoft license 
    agreement under which you licensed this sample source code and is provided AS-IS.
    If you did not accept the terms of the license agreement, you are not authorized 
    to use this sample source code.  For the terms of the license, please see the 
    license agreement between you and Microsoft.
  
    To see all Code Samples for Windows Phone, visit http://go.microsoft.com/fwlink/?LinkID=219604 
  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using Windows.ApplicationModel.Store;

namespace MockIAPLib
{
    public static class MockIAP
    {
        private static bool initalized;
        public static bool MockMode = true;
        private static ListingInformation _appListingInformation;
        public static Dictionary<string, ProductListing> allProducts;
        public static LicenseInformation _appLicenseInformation;
        public static MockReceiptState StateInformation;

        public static void CheckIfInitialized()
        {
            if (!initalized)
                throw new Exception("MockIAP was not initialized! Please make sure to call MockInit.Initialize first");
        }

        public static void Init()
        {
            if (initalized)
                return;


            _appLicenseInformation = new LicenseInformation
                                         {
                                             ExpirationDate = DateTimeOffset.Now,
                                             IsActive = true,
                                             IsTrial = false,
                                             ProductLicenses = new Dictionary<string, ProductLicense>()
                                         };

            StateInformation = new MockReceiptState();

            initalized = true;
        }

        public static void RunInMockMode(bool mockMode)
        {
            CheckIfInitialized();

            MockMode = mockMode;
        }

        public static void SetListingInformation(uint AgeRating, string CurrentMarket, string Description, string FormattedPrice, string Name)
        {
            CheckIfInitialized();

            if (_appListingInformation != null)
                throw new Exception("ListingInformation already created.");

            _appListingInformation = new ListingInformation
                                         {
                                             AgeRating = AgeRating,
                                             CurrentMarket = CurrentMarket,
                                             Description = Description,
                                             FormattedPrice = FormattedPrice,
                                             Name = Name,
                                             ProductListings = new Dictionary<string, ProductListing>()
                                         };
        }

        /// <summary>
        /// Populates the Mock Library with IAP items defined in XML
        /// </summary>
        /// <param name="Xml">XML to parse</param>
        public static void PopulateIAPItemsFromXml(string Xml)
        {
            CheckIfInitialized();

            XElement xmlDoc = XElement.Parse(Xml);

            foreach (XElement element in xmlDoc.Elements())
            {
                var pl = new ProductListing();

                pl.Name = element.Element("Name").Value;
                pl.ProductId = element.Element("ProductId").Value;
                pl.Description = element.Element("Description").Value;
                pl.FormattedPrice = element.Element("FormattedPrice").Value;
                string uri = element.Element("ImageUri").Value;
                pl.ImageUri = string.IsNullOrEmpty(uri) ? null : new Uri(uri);
                pl.ProductType = (ProductType) Enum.Parse(typeof (ProductType), element.Element("ProductType").Value);
                string keywords = element.Element("Keywords").Value;
                pl.Keywords = string.IsNullOrEmpty(keywords) ? null : keywords.Split(';');
                pl.Tag = element.Element("Tag").Value;

                if (pl.Tag.Length > 3000)
                    throw new Exception("Data stored in the 'Tag' can not exceed 3000 characters!");

                AddProductListing(element.Attribute("Key").Value, pl);

                bool purchased = bool.Parse(element.Attribute("Purchased").Value);
                bool fulfilled = bool.Parse(element.Attribute("Fulfilled").Value);

                if (!purchased && fulfilled)
                    throw new InvalidOperationException("Error in your XML definition: An item can't be marked as fulfilled but not purchased! Item: " + pl.Name);

                if (purchased)
                {
                    var store = new MockReceiptStore();
                    store.SaveReceipt(pl, false);
                    StateInformation.SetState(pl.ProductId, fulfilled);
                    _appLicenseInformation.ProductLicenses[pl.ProductId].IsActive = true;
                }
            }
        }

        public static void AddProductListing(string key, ProductListing productListing)
        {
            CheckIfInitialized();

            if (_appListingInformation == null)
                throw new Exception("A call to SetListingInformation is required before calling this method");

            if (allProducts == null)
                allProducts = new Dictionary<string, ProductListing>();

            allProducts.Add(key, productListing);

            var store = new MockReceiptStore();
            Dictionary<string, string> receipts = store.EnumerateReceipts();

            // add a license for this item as well. 
            var license = new ProductLicense
                              {
                                  ExpirationDate = DateTimeOffset.Now,
                                  IsActive = receipts.ContainsKey(productListing.ProductId),
                                  IsConsumable = productListing.ProductType == ProductType.Consumable,
                                  ProductId = productListing.ProductId
                              };

            _appLicenseInformation.ProductLicenses.Add(productListing.ProductId, license);
        }

        public static ListingInformation GetListingInformation()
        {
            return _appListingInformation;
        }

        internal static string SimulatePurchase(string ProductId, bool includeReceipt)
        {
            ProductListing listing = allProducts.Values.FirstOrDefault(p => p.ProductId.Equals(ProductId, StringComparison.InvariantCultureIgnoreCase));

            if (listing == null)
                throw new ArgumentException("Specified productId has no ProductListing");

            string receipt = string.Empty;
            bool OkToPurchase = true;

            bool? state = StateInformation.GetState(listing.ProductId);
            if (state != null && state.Value == false)
            {
                // This is an unfulfiled item
                MessageBox.Show("You have already purchased this but not fulfiled it yet");
                OkToPurchase = false;
            }
            else
            {
                var rs = new MockReceiptStore();
                if (listing.ProductType == ProductType.Durable && rs.EnumerateReceipts().ContainsKey(listing.ProductId))
                {
                    MessageBox.Show("You already purchased this durable");
                    OkToPurchase = false;
                }
            }

            if (OkToPurchase)
            {
                MessageBoxResult result = MessageBox.Show(string.Format("Simulating purchase. Do you want to buy this item ({0})?", listing.Name), "Mock UI", MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                {
                    var store = new MockReceiptStore();
                    receipt = store.SaveReceipt(listing, includeReceipt);
                    StateInformation.SetState(listing.ProductId, listing.ProductType == ProductType.Durable); // Set as fulfilled for Durables only 
                    _appLicenseInformation.ProductLicenses[listing.ProductId].IsActive = true;
                }
                else
                {
                    throw new Exception("User has clicked on Cancel. In the real API, an exception will be thrown if that happens as well. You must put a try/catch around the RequestProductPurchaseAsync call to handle this.");
                }
            }

            return receipt;
        }

        internal static void ReportFulfillment(string selectedProductId)
        {
            ProductListing listing = allProducts.Values.FirstOrDefault(p => p.ProductId.Equals(selectedProductId, StringComparison.InvariantCultureIgnoreCase));

            if (listing == null)
                throw new Exception(string.Format("Specified ProductID ({0}) does not exist", selectedProductId));

            switch (listing.ProductType)
            {
                case ProductType.Durable:
                    {
                        throw new InvalidOperationException("Fulfillment is only applicable to Consumables");
                    }
                case ProductType.Unknown:
                    {
                        throw new InvalidOperationException("ProductType is Unknown");
                    }
                case ProductType.Consumable:
                    {
                        StateInformation.SetState(selectedProductId, true);
                        var rs = new MockReceiptStore();
                        rs.DeleteReceipt(selectedProductId);
                        _appLicenseInformation.ProductLicenses[selectedProductId].IsActive = false;
                        break;
                    }
            }
        }

        public static void ClearCache()
        {
            StateInformation.ClearCache();
            var rs = new MockReceiptStore();
            foreach (var lic in _appLicenseInformation.ProductLicenses.Values)
            {
                lic.IsActive = false;
            }
            rs.ClearStore();
        }
    }
}
#endif