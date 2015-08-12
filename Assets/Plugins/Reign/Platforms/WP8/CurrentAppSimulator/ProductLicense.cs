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

namespace MockIAPLib
{
    // Summary:
    //     Provides info about a license that is associated with an in-app offer.
    public sealed class ProductLicense
    {
        // Summary:
        //     Not implemented for Windows Phone. Gets the expiration date and time of the
        //     license.
        //
        // Returns:
        //     The date and time that the product's license expires.
        public DateTimeOffset ExpirationDate { get; set; }
        //
        // Summary:
        //     Gets the value that indicates whether the feature's license is active.
        //
        // Returns:
        //     Returns true if the feature's license is active, and otherwise false. This
        //     property may return false if the license is missing, expired, or revoked.
        public bool IsActive { get; set; }
        //
        // Summary:
        //     Gets whether this product is a consumable. A consumable product is a product
        //     that can be purchased, used, and purchased again.
        //
        // Returns:
        //     true if the product is a consumable; Otherwise, false.
        public bool IsConsumable { get; set; }
        //
        // Summary:
        //     Gets the ID of an app's in-app offer.
        //
        // Returns:
        //     The ID specified in the Dashboard to identify this in-app offer.
        public string ProductId { get; set; }

        public static ProductLicense Create(Windows.ApplicationModel.Store.ProductLicense source)
        {
            var license = new ProductLicense
                              {
                                  ExpirationDate = source.ExpirationDate,
                                  IsConsumable = source.IsConsumable,
                                  IsActive = source.IsActive,
                                  ProductId = source.ProductId
                              };

            return license;
        }
    }
}
#endif