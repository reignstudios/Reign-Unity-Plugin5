package com.reignstudios.reignnativeamazon;

import java.io.File;
import java.io.FileWriter;
import java.util.ArrayList;
import java.util.HashSet;
import java.util.List;
import java.util.Map;
import java.util.Set;

import org.json.JSONObject;

import android.content.Intent;
import android.os.AsyncTask;
import android.os.Environment;
import android.util.Log;


import com.amazon.device.iap.PurchasingListener;
import com.amazon.device.iap.PurchasingService;
import com.amazon.device.iap.model.FulfillmentResult;
import com.amazon.device.iap.model.Product;
import com.amazon.device.iap.model.ProductDataResponse;
import com.amazon.device.iap.model.PurchaseResponse;
import com.amazon.device.iap.model.PurchaseUpdatesResponse;
import com.amazon.device.iap.model.UserData;
import com.amazon.device.iap.model.UserDataResponse;
import com.amazon.device.iap.model.Receipt;

import com.reignstudios.reignnative.ReignActivityCallbacks;
import com.reignstudios.reignnative.ReignAndroidPlugin;

public class Amazon_InAppPurchaseNative implements PurchasingListener, ReignActivityCallbacks
{
	private static final String logTag = "Reign_Amazon_IAP";
	private static Amazon_InAppPurchaseNative Singleton;
	private static boolean buyDone, buySuccess, restoreDone, productInfoDone;
	private static List<String> restoreItems, productInfoItems;
	private static Set<String> skuSet;
	private static int initStatus;
	private static String buyReceipt;
	
	public static void Init(String itemSKUs, String itemTypes, boolean testing)
	{
		if (testing)
		{
			try
			{
				File sdCard = Environment.getExternalStorageDirectory();
				File dir = new File (sdCard.getAbsolutePath());
				dir.mkdirs();
				
				// get SKUs
				String[] skus = itemSKUs.split(":");
				String[] types = itemTypes.split(":");
				JSONObject rootObj = new JSONObject();
				JSONObject obj = new JSONObject();
				skuSet = new HashSet<String>();
				for (int i = 0; i != skus.length; ++i)
				{
					obj.put("itemType", types[i]);
					obj.put("price", 0.99);
					obj.put("title", skus[i]);
					obj.put("description", skus[i] + " Desc will go here...");
					obj.put("smallIconUrl",  "http://reign-studios-services.com/ReignUnityPluginResources/amazon-Apps-icon.png");
					rootObj.put(skus[i], obj);
					skuSet.add(skus[i]);
				}
				
				FileWriter file = new FileWriter(dir + "/amazon.sdktester.json");
				file.write(rootObj.toString());
				file.flush();
				file.close();
				
				initStatus = 1;
			}
			catch (Exception e)
			{
				Log.d(logTag, "Failed to create Test InApp json file." + e.getMessage());
				initStatus = 2;
			}
		}
		else
		{
			String[] skus = itemSKUs.split(":");
			skuSet = new HashSet<String>();
			for (int i = 0; i != skus.length; ++i)
			{
				skuSet.add(skus[i]);
			}
		}
		
		Singleton = new Amazon_InAppPurchaseNative();
		ReignAndroidPlugin.AddCallbacks(Singleton);
		PurchasingService.registerListener(ReignAndroidPlugin.RootActivity, Singleton);
		if (!testing) initStatus = 1;
	}
	
	public static int CheckInitStatus()
	{
		int status = initStatus;
		initStatus = 0;
		return status;
	}
	
	@Override
	public boolean onActivityResult(int requestCode, int resultcode, Intent intent)
	{
		return false;
	}
	
	@Override
	public void onPause()
	{
		// do nothing...
	}
	
	@Override
	public void onResume()
	{
		PurchasingService.getUserData();
	}
	
	public static void BuyApp(final String sku)
	{
		ReignAndroidPlugin.RootActivity.runOnUiThread(new Runnable()
		{
			public void run()
			{
				PurchasingService.purchase(sku);
			}
		});
	}
	
	public static boolean CheckBuyDone()
	{
		boolean done = buyDone;
		buyDone = false;
		return done;
	}
	
	public static boolean CheckBuySuccess()
	{
		return buySuccess;
	}
	
	public static String GetBuyReceipt()
	{
		return buyReceipt;
	}
	
	public static void Restore()
	{
		restoreItems = new ArrayList<String>();
		restoreDone = false;

		ReignAndroidPlugin.RootActivity.runOnUiThread(new Runnable()
		{
			public void run()
			{
				PurchasingService.getPurchaseUpdates(false);
			}
		});
	}
	
	public static boolean CheckRestoreDone()
	{
		boolean done = restoreDone;
		if (restoreDone) restoreDone = false;
		return done;
	}
	
	public static String[] GetRestoreItems()
    {
    	return restoreItems.toArray(new String[restoreItems.size()]);
    }
	
	public static void GetProductInfo()
	{
		productInfoItems = new ArrayList<String>();
		productInfoDone = false;

		ReignAndroidPlugin.RootActivity.runOnUiThread(new Runnable()
		{
			public void run()
			{
				PurchasingService.getProductData(skuSet);
			}
		});
	}
	
	public static boolean CheckProductInfoDone()
	{
		boolean done = productInfoDone;
		if (productInfoDone) productInfoDone = false;
		return done;
	}
	
	public static String[] GetProductInfoItems()
    {
    	return productInfoItems.toArray(new String[productInfoItems.size()]);
    }
	
	// ===============================
	// PurchasingListener
	// ===============================
	//private String currentUser;
	
	@Override
	public void onProductDataResponse(ProductDataResponse arg)
	{
		Log.v(logTag, "onItemDataResponse recieved");
        Log.v(logTag, "ItemDataRequestStatus" + arg.getRequestStatus());
        Log.v(logTag, "ItemDataRequestId" + arg.getRequestId());
        new ItemDataAsyncTask().execute(arg);
	}

	@Override
	public void onPurchaseResponse(PurchaseResponse arg)
	{
		Log.v(logTag, "onPurchaseResponse recieved");
        Log.v(logTag, "PurchaseRequestStatus:" + arg.getRequestStatus());
        new PurchaseAsyncTask().execute(arg);
	}

	@Override
	public void onPurchaseUpdatesResponse(PurchaseUpdatesResponse arg)
	{
		Log.v(logTag, "onPurchaseUpdatesRecived recieved: Response -" + arg);
        Log.v(logTag, "PurchaseUpdatesRequestStatus:" + arg.getRequestStatus());
        Log.v(logTag, "RequestID:" + arg.getRequestId());
        new PurchaseUpdatesAsyncTask().execute(arg);
	}

	@Override
	public void onUserDataResponse(UserDataResponse arg)
	{
		UserData data = arg.getUserData();
		//currentUser = data.getUserId();
		Log.v(logTag, "onGetUserIdResponse recieved: Response -" + data.getUserId());
        Log.v(logTag, "RequestId:" + arg.getRequestId());
        Log.v(logTag, "IdRequestStatus:" + arg.getRequestStatus());
        //new GetUserIdAsyncTask().execute(data.getUserId());
	}
    
    
    // ===============================
 	// PurchasingListener Helpers
 	// ===============================
    /*private void printReceipt(final Receipt receipt)
    {
        Log.v
        (
        	logTag,
            String.format("Receipt: ItemType: %s Sku: %s PurchaseDate: %s", receipt.getProductType(),
            receipt.getSku(), receipt.getPurchaseDate())
        );
    }*/

    /*
     * Started when the Observer receives a GetUserIdResponse. The Shared Preferences file for the returned user id is
     * accessed.
     */
    /*private class GetUserIdAsyncTask extends AsyncTask<GetUserIdResponse, Void, Boolean>
    {

        @Override
        protected Boolean doInBackground(final GetUserIdResponse... params)
        {
            GetUserIdResponse getUserIdResponse = params[0];

            if (getUserIdResponse.getUserIdRequestStatus() == GetUserIdRequestStatus.SUCCESSFUL)
            {
                final String userId = getUserIdResponse.getUserId();

                // Each UserID has their own shared preferences file, and we'll load that file when a new user logs in.
                setCurrentUser(userId);
                return true;
            }
            else
            {
                Log.v(logTag, "onGetUserIdResponse: Unable to get user ID.");
                return false;
            }
        }*/

        /*
         * Call initiatePurchaseUpdatesRequest for the returned user to sync purchases that are not yet fulfilled.
         */
        /*@Override
        protected void onPostExecute(final Boolean result)
        {
            super.onPostExecute(result);
            if (result)
            {
                // do nothing...
            }
        }
    }*/

    /*
     * Started when the observer receives an Item Data Response.
     * Takes the items and display them in the logs. You can use this information to display an in game
     * storefront for your IAP items.
     */
    private class ItemDataAsyncTask extends AsyncTask<ProductDataResponse, Void, Void>
    {
        @Override
        protected Void doInBackground(final ProductDataResponse... params)
        {
            final ProductDataResponse itemDataResponse = params[0];

            switch (itemDataResponse.getRequestStatus())
            {
            case SUCCESSFUL:
                // Information you'll want to display about your IAP items is here
                // In this example we'll simply log them.
                final Map<String, Product> items = itemDataResponse.getProductData();
                for (final String key : items.keySet())
                {
                	Product i = items.get(key);
                    Log.v(logTag, String.format("Item: %s\n Type: %s\n SKU: %s\n Price: %s\n Description: %s\n", i.getTitle(), i.getProductType(), i.getSku(), i.getPrice(), i.getDescription()));
                    productInfoItems.add(key);
                    String price = i.getPrice();
                    productInfoItems.add(price);
                }
                productInfoDone = true;
                break;
                
            case FAILED:
            	productInfoItems.clear();
            	productInfoDone = true;
                break;

            case NOT_SUPPORTED:
            	// Skus that you can not purchase will be here.
                for (final String s : itemDataResponse.getUnavailableSkus())
                {
                    Log.v(logTag, "Unavailable SKU:" + s);
                }
                productInfoItems.clear();
            	productInfoDone = true;
            	break;
            }

            return null;
        }
    }

    /*
     * Started when the observer receives a Purchase Response
     * Once the AsyncTask returns successfully, the UI is updated.
     */
    private class PurchaseAsyncTask extends AsyncTask<PurchaseResponse, Void, Boolean>
    {
        @Override
        protected Boolean doInBackground(final PurchaseResponse... params)
        {
            final PurchaseResponse purchaseResponse = params[0];            
           // final String userId = getCurrentUser();
            
            /*if (!purchaseResponse.getUserId().equals(userId))
            {
                // currently logged in user is different than what we have so update the state
                setCurrentUser(purchaseResponse.getUserId());                
                PurchasingManager.initiatePurchaseUpdatesRequest(Offset.fromString(baseActivity.getSharedPreferences(getCurrentUser(), Context.MODE_PRIVATE).getString(OFFSET, Offset.BEGINNING.toString())));                
            }*/
            
            switch (purchaseResponse.getRequestStatus())
            {
            case SUCCESSFUL:
                /*
                 * You can verify the receipt and fulfill the purchase on successful responses.
                 */
                /*final Receipt receipt = purchaseResponse.getReceipt();
                switch (receipt.getProductType())
                {
                case CONSUMABLE:
                	buySuccess = true;
                	buyDone = true;
                    break;
                case ENTITLED:
                	buySuccess = true;
                	buyDone = true;
                    break;
                case SUBSCRIPTION:
                	buySuccess = true;
                	buyDone = true;
                    break;
                }*/

                //printReceipt(purchaseResponse.getReceipt());
            	Receipt receipt = purchaseResponse.getReceipt();
            	buyReceipt = receipt.toJSON().toString();
            	PurchasingService.notifyFulfillment(receipt.getReceiptId(), FulfillmentResult.FULFILLED);
            	Log.d(logTag, "Successful purchase.");
                buySuccess = true;
            	buyDone = true;
                return true;
                
            case ALREADY_PURCHASED:
                /*
                 * If the customer has already been entitled to the item, a receipt is not returned.
                 * Fulfillment is done unconditionally, we determine which item should be fulfilled by matching the
                 * request id returned from the initial request with the request id stored in the response.
                 */
            	Log.d(logTag, "Entitled already purchased.");
            	buySuccess = true;
            	buyDone = true;
                return true;
                
            case FAILED:
                /*
                 * If the purchase failed for some reason, (The customer canceled the order, or some other
                 * extraneous circumstance happens) the application ignores the request and logs the failure.
                 */
            	Log.d(logTag, "Failed to purchase.");
            	buySuccess = false;
            	buyDone = true;
                return false;
                
            case INVALID_SKU:
                /*
                 * If the sku that was purchased was invalid, the application ignores the request and logs the failure.
                 * This can happen when there is a sku mismatch between what is sent from the application and what
                 * currently exists on the dev portal.
                 */
            	Log.d(logTag, "Invalid sku.");
            	buySuccess = false;
            	buyDone = true;
                return false;
                
            case NOT_SUPPORTED:
            	Log.d(logTag, "Failed to purchase (Not Supported).");
            	buySuccess = false;
            	buyDone = true;
            	return false;
            }
            
            Log.d(logTag, "Failed to purchase (Unknown Error).");
            buySuccess = false;
        	buyDone = true;
            return false;
        }

        @Override
        protected void onPostExecute(final Boolean success)
        {
            super.onPostExecute(success);
            if (success)
            {
                // do nothing...
            }
        }
    }

    /*
     * Started when the observer receives a Purchase Updates Response Once the AsyncTask returns successfully, we'll
     * update the UI.
     */
    private class PurchaseUpdatesAsyncTask extends AsyncTask<PurchaseUpdatesResponse, Void, Boolean>
    {

        @Override
        protected Boolean doInBackground(final PurchaseUpdatesResponse... params)
        {
            final PurchaseUpdatesResponse purchaseUpdatesResponse = params[0];
            //final SharedPreferences.Editor editor = getSharedPreferencesEditor();
            /*final String userId = getCurrentUser();
            if (!purchaseUpdatesResponse.getUserId().equals(userId)) {
                return false;
            }*/
            
            /*
             * If the customer for some reason had items revoked, the skus for these items will be contained in the
             * revoked skus set.
             */
            /*for (final String sku : purchaseUpdatesResponse.getRevokedSkus())
            {
                Log.v(logTag, "Revoked Sku:" + sku);
            }*/

            switch (purchaseUpdatesResponse.getRequestStatus())
            {
            case SUCCESSFUL:
                //SubscriptionPeriod latestSubscriptionPeriod = null;
                //final LinkedList<SubscriptionPeriod> currentSubscriptionPeriods = new LinkedList<SubscriptionPeriod>();
                for (final Receipt receipt : purchaseUpdatesResponse.getReceipts())
                {
                    final String sku = receipt.getSku();
                    //final String key = getKey(sku);
                    switch (receipt.getProductType())
                    {
                    case ENTITLED:
                        /*
                         * If the receipt is for an entitlement, the customer is re-entitled.
                         */
                    	restoreItems.add(sku);
                        break;
                        
                    case CONSUMABLE:
                    	// do nothing..
                    	break;
                        
                    case SUBSCRIPTION:
                        /*
                         * Purchase Updates for subscriptions can be done in one of two ways:
                         * 1. Use the receipts to determine if the user currently has an active subscription
                         * 2. Use the receipts to create a subscription history for your customer.
                         * This application checks if there is an open subscription the application uses the receipts
                         * returned to determine an active subscription.
                         * Applications that unlock content based on past active subscription periods, should create
                         * purchasing history for the customer.
                         * For example, if the customer has a magazine subscription for a year,
                         * even if they do not have a currently active subscription,
                         * they still have access to the magazines from when they were subscribed.
                         */
                        //final SubscriptionPeriod subscriptionPeriod = receipt.getPurchaseDate();
                        //if (subscriptionPeriod.getEndDate() == null) restoreItems.add(sku);
                        break;
                    }
                
                    //printReceipt(receipt);
                }

                /*
                 * Store the offset into shared preferences. If there has been more purchases since the
                 * last time our application updated, another initiatePurchaseUpdatesRequest is called with the new
                 * offset.
                 */
                //final Offset newOffset = purchaseUpdatesResponse.getOffset();
                //editor.putString(OFFSET, newOffset.toString());
                //editor.commit();
                if (purchaseUpdatesResponse.hasMore())
                {
                    Log.v(logTag, "Initiating Another Purchase Updates with offset");// + newOffset.toString());
                    PurchasingService.getPurchaseUpdates(false);
                }
                else
                {
                	restoreDone = true;
                }
                return true;
                
            case FAILED:
                /*
                 * On failed responses the application will ignore the request.
                 */
                restoreDone = true;
                return false;
                
            case NOT_SUPPORTED:
            	restoreDone = true;
            	return false;
            }
            return false;
        }

        @Override
        protected void onPostExecute(final Boolean success)
        {
            super.onPostExecute(success);
            if (success)
            {
            	// do nothing...
            }
        }
    }
}
