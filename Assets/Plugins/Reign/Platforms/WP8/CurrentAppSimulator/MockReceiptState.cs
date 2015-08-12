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
using System.Linq;
using System.Xml.Serialization;

namespace MockIAPLib
{
    public class MockReceiptState
    {
        private const string ReceiptStore = "MockReceiptStore";
        private List<ReceiptState> stateInformation;

        public MockReceiptState()
        {
            IsolatedStorageFile userStore = IsolatedStorageFile.GetUserStoreForApplication();
            if (!userStore.DirectoryExists(ReceiptStore))
                userStore.CreateDirectory(ReceiptStore);

            LoadState();
        }

        private void LoadState()
        {
            IsolatedStorageFile userStore = IsolatedStorageFile.GetUserStoreForApplication();
            string stateFile = Path.Combine(ReceiptStore, "state.xml");
            if (userStore.FileExists(stateFile))
            {
                var serializer = new XmlSerializer(typeof (List<ReceiptState>));
                IsolatedStorageFileStream stream = userStore.OpenFile(stateFile, FileMode.Open);
                stateInformation = (List<ReceiptState>) serializer.Deserialize(stream);
                stream.Close();
            }
            else
            {
                stateInformation = new List<ReceiptState>();
            }
        }

        public void SaveState()
        {
            if (stateInformation != null && stateInformation.Count > 0)
            {
                IsolatedStorageFile userStore = IsolatedStorageFile.GetUserStoreForApplication();
                string stateFile = Path.Combine(ReceiptStore, "state.xml");
                if (userStore.FileExists(stateFile))
                    userStore.DeleteFile(stateFile);

                var serializer = new XmlSerializer(typeof (List<ReceiptState>));
                IsolatedStorageFileStream stream = userStore.OpenFile(stateFile, FileMode.CreateNew);
                serializer.Serialize(stream, stateInformation);
                stream.Close();
            }
        }

        public void SetState(string ProductID, bool state)
        {
            if (stateInformation != null)
            {
                ReceiptState target = stateInformation.FirstOrDefault(s => s.productID.Equals(ProductID, StringComparison.InvariantCultureIgnoreCase));
                if (target == null)
                    stateInformation.Add(new ReceiptState {productID = ProductID, ack = state});
                else
                    target.ack = state;

                SaveState();
            }
        }

        public bool? GetState(string ProductID)
        {
            bool? result;
            ReceiptState target = stateInformation.FirstOrDefault(s => s.productID.Equals(ProductID, StringComparison.InvariantCultureIgnoreCase));

            if (target == null)
                result = null;
            else
                result = target.ack;

            return result;
        }

        public void ClearCache()
        {
            IsolatedStorageFile userStore = IsolatedStorageFile.GetUserStoreForApplication();
            string stateFile = Path.Combine(ReceiptStore, "state.xml");
            if (userStore.FileExists(stateFile))
                userStore.DeleteFile(stateFile);
        }

        #region Nested type: ReceiptState

        public class ReceiptState
        {
            public bool ack;
            public string productID;
        }

        #endregion
    }
}
#endif