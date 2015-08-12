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
using System.Threading.Tasks;

namespace MockIAPLib
{
    public class CurrentApp
    {
        private static LicenseInformation _licInfo;

        public static LicenseInformation LicenseInformation
        {
            get { return _licInfo ?? (_licInfo = MockIAP._appLicenseInformation); }
            set { _licInfo = value; }
        }

        public static Guid AppId
        {
            get { return Windows.ApplicationModel.Store.CurrentApp.AppId; }
        }

        public static Uri LinkUri
        {
            get { return Windows.ApplicationModel.Store.CurrentApp.LinkUri; }
        }

        public static async Task<string> GetAppReceiptAsync()
        {
            return await Windows.ApplicationModel.Store.CurrentApp.GetAppReceiptAsync();
        }

        public static async Task<string> GetProductReceiptAsync(string selectedProductId)
        {
            MockIAP.CheckIfInitialized();

            string receipt = null;

            if (!MockIAP.MockMode)
            {
                receipt = await Windows.ApplicationModel.Store.CurrentApp.GetProductReceiptAsync(selectedProductId);
            }
            else
            {
                var rs = new MockReceiptStore();
                Dictionary<string, string> receipts = rs.EnumerateReceipts();
                if (receipts.ContainsKey(selectedProductId))
                    receipt = receipts[selectedProductId];
            }

            return receipt;
        }

        public static async Task<ListingInformation> LoadListingInformationAsync()
        {
            MockIAP.CheckIfInitialized();

            ListingInformation listingInformation;

            if (!MockIAP.MockMode)
            {
                Windows.ApplicationModel.Store.ListingInformation li = await Windows.ApplicationModel.Store.CurrentApp.LoadListingInformationAsync();
                listingInformation = new ListingInformation(li);

                LicenseInformation = new LicenseInformation(Windows.ApplicationModel.Store.CurrentApp.LicenseInformation);
            }
            else
            {
                listingInformation = MockIAP.GetListingInformation();
                listingInformation.ProductListings = MockIAP.allProducts;
            }

            return listingInformation;
        }

        public static async Task<ListingInformation> LoadListingInformationByProductIdsAsync(string[] ProductIds)
        {
            MockIAP.CheckIfInitialized();

            ListingInformation listingInformation;

            if (!MockIAP.MockMode)
            {
                Windows.ApplicationModel.Store.ListingInformation li = await Windows.ApplicationModel.Store.CurrentApp.LoadListingInformationByProductIdsAsync(ProductIds);
                listingInformation = new ListingInformation(li);
            }
            else
            {
                listingInformation = MockIAP.GetListingInformation();
                listingInformation.ProductListings = new Dictionary<string, ProductListing>();

                IEnumerable<string> result = from key in MockIAP.allProducts.Keys
                                             join pId in ProductIds on MockIAP.allProducts[key].ProductId equals pId
                                             select key;

                result.ToList().ForEach(k => listingInformation.ProductListings.Add(k, MockIAP.allProducts[k]));
            }

            return listingInformation;
        }

        public static async Task<ListingInformation> LoadListingInformationByKeywordsAsync(string[] ProductKeywords)
        {
            MockIAP.CheckIfInitialized();

            ListingInformation listingInformation;

            if (!MockIAP.MockMode)
            {
                Windows.ApplicationModel.Store.ListingInformation li = await Windows.ApplicationModel.Store.CurrentApp.LoadListingInformationByProductIdsAsync(ProductKeywords);
                listingInformation = new ListingInformation(li);
            }
            else
            {
                listingInformation = MockIAP.GetListingInformation();
                listingInformation.ProductListings = new Dictionary<string, ProductListing>();

                IEnumerable<string> result = from key in MockIAP.allProducts.Keys
                                             from keyword in ProductKeywords
                                             where MockIAP.allProducts[key].Keywords.Contains(keyword)
                                             select key;

                result.ToList().ForEach(k => listingInformation.ProductListings.Add(k, MockIAP.allProducts[k]));
            }

            return listingInformation;
        }

        public static void ReportProductFulfillment(string selectedProductId)
        {
            MockIAP.CheckIfInitialized();

            if (!MockIAP.MockMode)
            {
                Windows.ApplicationModel.Store.CurrentApp.ReportProductFulfillment(selectedProductId);
            }
            else
            {
                MockIAP.ReportFulfillment(selectedProductId);
            }
        }

        public static async Task<string> RequestProductPurchaseAsync(string ProductId, bool includeReceipt)
        {
            MockIAP.CheckIfInitialized();

            string receipt;

            if (!MockIAP.MockMode)
            {
                receipt = await Windows.ApplicationModel.Store.CurrentApp.RequestProductPurchaseAsync(ProductId, includeReceipt);

                // Now we refresh the license information post purchase.
                LicenseInformation = new LicenseInformation(Windows.ApplicationModel.Store.CurrentApp.LicenseInformation);
            }
            else
            {
                receipt = MockIAP.SimulatePurchase(ProductId, includeReceipt);
            }

            return receipt;
        }

        public static async Task<string> RequestAppPurchaseAsync(bool includeReceipt)
        {
            return await Windows.ApplicationModel.Store.CurrentApp.RequestAppPurchaseAsync(includeReceipt);
        }
    }
}
#endif