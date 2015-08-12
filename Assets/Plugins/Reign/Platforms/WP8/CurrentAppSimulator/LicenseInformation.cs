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
using Windows.ApplicationModel.Store;

namespace MockIAPLib
{
    // Summary:
    //     Provides access to the current app's license metadata.
    public sealed class LicenseInformation
    {
        // Summary:
        //     Not implemented for Windows Phone. Gets the license expiration date and time
        //     relative to the system clock.
        //
        // Returns:
        //     The date and time that the app's trial license will expire.
        public LicenseInformation(Windows.ApplicationModel.Store.LicenseInformation li)
        {
            IsActive = li.IsActive;
            IsTrial = li.IsTrial;
            ExpirationDate = li.ExpirationDate;
            ProductLicenses = new Dictionary<string, ProductLicense>();

            foreach (string key in li.ProductLicenses.Keys)
            {
                ProductLicenses.Add(key, ProductLicense.Create(li.ProductLicenses[key]));
            }
        }

        public LicenseInformation()
        {
            // TODO: Complete member initialization
        }

        public DateTimeOffset ExpirationDate { get; set; }
        //
        // Summary:
        //     Gets the value that indicates whether the license is active.
        //
        // Returns:
        //     Returns true if the license is active, and otherwise false. May return false
        //     if the license is missing, expired, or revoked.
        public bool IsActive { get; set; }
        //
        // Summary:
        //     Gets the value that indicates whether the license is a trial license.
        //
        // Returns:
        //     Returns true if the license is a trial license, and otherwise false.
        public bool IsTrial { get; set; }
        //
        // Summary:
        //     Gets the associative list of licenses for the app's features that can be
        //     bought through an in-app purchase.
        //
        // Returns:
        //     The associative list of feature licenses.
        public Dictionary<string, ProductLicense> ProductLicenses { get; set; }

        // Summary:
        //     Not implemented for Windows Phone. Raises a notification event when the status
        //     of the app's license changes.
        public event LicenseChangedEventHandler LicenseChanged;
    }
}
#endif