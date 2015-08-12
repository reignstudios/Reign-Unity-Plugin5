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
using System.Collections.Generic;

namespace MockIAPLib
{
    public sealed class ListingInformation
    {
        // Summary:
        //     Not implemented for Windows Phone. Gets the age rating for the app.
        //
        // Returns:
        //     The age rating.
        public ListingInformation()
        {
        }

        public ListingInformation(Windows.ApplicationModel.Store.ListingInformation source)
        {
            AgeRating = source.AgeRating;
            CurrentMarket = source.CurrentMarket;
            Description = source.Description;
            FormattedPrice = source.FormattedPrice;
            Name = source.Name;
            ProductListings = new Dictionary<string, ProductListing>();

            foreach (string key in source.ProductListings.Keys)
            {
                ProductListings.Add(key, ProductListing.Create(source.ProductListings[key]));
            }
        }

        public uint AgeRating { get; set; }
        //
        // Summary:
        //     Not implemented for Windows Phone. Gets the value that indicates the customer's
        //     market, such as en-us, that will be used for transactions in the current
        //     session.
        //
        // Returns:
        //     The value that indicates the customer's market, such as en-us.
        public string CurrentMarket { get; set; }
        //
        // Summary:
        //     Gets the app's description in the current market.
        //
        // Returns:
        //     The app's description in the current market.
        public string Description { get; set; }
        //
        // Summary:
        //     Gets the app's purchase price formatted for the current market and currency.
        //
        // Returns:
        //     The app's purchase price with the appropriate formatting for the current
        //     market and currency.
        public string FormattedPrice { get; set; }
        //
        // Summary:
        //     Gets the app's name in the current market.
        //
        // Returns:
        //     The app's name in the current market.
        public string Name { get; set; }
        //
        // Summary:
        //     Gets information about the features and products that can be bought by making
        //     in-app purchases.
        //
        // Returns:
        //     The array of ProductListing objects that describes the app's in-app offers.
        public Dictionary<string, ProductListing> ProductListings { get; set; }
    }
}
#endif