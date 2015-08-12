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
using System.IO;
using System.IO.IsolatedStorage;
using Windows.ApplicationModel.Store;

namespace MockIAPLib
{
    public class MockReceiptStore
    {
        private const string ReceiptStore = "MockReceiptStore";
        private const string ReceiptTemplate = @"<?xml version=""1.0""?><Receipt Version=""1.0"" CertificateId=""FB3D3A6455095D2C4A841AA8B8E20661B10A6112"" xmlns=""http://schemas.microsoft.com/windows/2012/store/receipt""><ProductReceipt PurchasePrice=""{0}"" PurchaseDate=""{1}"" Id=""{2}"" AppId=""bcece01c-42df-4f4c-8a9b-18bb5c49bc73"" ProductId=""{3}"" ProductType=""{4}"" PublisherUserId=""00000000000000000000000000000000000000000000"" PublisherDeviceId=""00000000-0000-0000-0000-000000000000"" MicrosoftProductId=""ca65ff27-5472-48a1-95ee-a49591390855"" /><Signature xmlns=""http://www.w3.org/2000/09/xmldsig#""><SignedInfo><CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315"" /><SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1"" /><Reference URI=""""><Transforms><Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature"" /></Transforms><DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1"" /><DigestValue>whocares</DigestValue></Reference></SignedInfo><SignatureValue>whocares</SignatureValue></Signature></Receipt>";

        public MockReceiptStore()
        {
            IsolatedStorageFile userStore = IsolatedStorageFile.GetUserStoreForApplication();
            if (!userStore.DirectoryExists(ReceiptStore))
                userStore.CreateDirectory(ReceiptStore);
        }

        public Dictionary<string, string> EnumerateReceipts()
        {
            var result = new Dictionary<string, string>();

            IsolatedStorageFile userStore = IsolatedStorageFile.GetUserStoreForApplication();
            foreach (string fileName in userStore.GetFileNames(ReceiptStore + @"\*.xml"))
            {
                string productId = Path.GetFileNameWithoutExtension(fileName);

                IsolatedStorageFileStream stream = userStore.OpenFile(Path.Combine(ReceiptStore, fileName), FileMode.Open);
                var sr = new StreamReader(stream);
                string contents = sr.ReadToEnd();
                sr.Close();

                result.Add(productId, contents);
            }

            return result;
        }

        public string SaveReceipt(ProductListing productListing, bool returnReceipt)
        {
            if (productListing.ProductType == ProductType.Unknown)
                return string.Empty; // nothing to do with this one.

            string mockReceipt = string.Format(ReceiptTemplate,
                                               productListing.FormattedPrice,
                                               DateTime.Now.ToLongTimeString(),
                                               Guid.NewGuid().ToString(),
                                               productListing.ProductId,
                                               productListing.ProductType.ToString());


            IsolatedStorageFile userStore = IsolatedStorageFile.GetUserStoreForApplication();
            string targetFileName = Path.Combine(ReceiptStore, productListing.ProductId + ".xml");

            if (!userStore.FileExists(targetFileName))
            {
                IsolatedStorageFileStream stream = userStore.CreateFile(targetFileName);
                var sw = new StreamWriter(stream);
                sw.WriteLine(mockReceipt);
                sw.Close();
            }

            return mockReceipt;
        }

        public void DeleteReceipt(string productId)
        {
            IsolatedStorageFile userStore = IsolatedStorageFile.GetUserStoreForApplication();
            string targetFileName = Path.Combine(ReceiptStore, productId + ".xml");

            if (userStore.FileExists(targetFileName))
            {
                userStore.DeleteFile(targetFileName);
            }
        }

        public void ClearStore()
        {
            IsolatedStorageFile userStore = IsolatedStorageFile.GetUserStoreForApplication();
            foreach (string file in userStore.GetFileNames(ReceiptStore + @"\*.xml"))
            {
                userStore.DeleteFile(Path.Combine(ReceiptStore, file));
            }
        }
    }
}
#endif