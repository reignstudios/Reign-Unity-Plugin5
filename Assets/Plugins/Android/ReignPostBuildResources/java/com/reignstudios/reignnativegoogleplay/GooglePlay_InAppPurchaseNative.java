package com.reignstudios.reignnativegoogleplay;

import java.util.ArrayList;
import java.util.List;

import android.content.Intent;
import android.util.Log;

import com.example.android.trivialdrivesample.util.IabHelper;
import com.example.android.trivialdrivesample.util.IabResult;
import com.example.android.trivialdrivesample.util.Inventory;
import com.example.android.trivialdrivesample.util.Purchase;
import com.example.android.trivialdrivesample.util.Security;
import com.example.android.trivialdrivesample.util.SkuDetails;

import com.reignstudios.reignnative.ReignActivityCallbacks;
import com.reignstudios.reignnative.ReignAndroidPlugin;

public class GooglePlay_InAppPurchaseNative implements ReignActivityCallbacks
{
	private static final String logTag = "Reign_GooglePlay_IAP";
	private static GooglePlay_InAppPurchaseNative singleton;
	public static int BuyIntentID = 1234;
	private static boolean restoreDone, buyDone, buySuccess, productInfoDone;
	private static List<String> restoreItems, productInfoItems;
	private static String buyItem, base64Key, buyReceipt;
	private static String[] skus, types;
	private static List<String> skuList;
	private static IabHelper helper;
	private static int initStatus;

	public static void Init(final String base64Key, String itemSKUs, String itemTypes, final boolean testing, final boolean clearCache)
	{
		// attached callbacks
		singleton = new GooglePlay_InAppPurchaseNative();
		ReignAndroidPlugin.AddCallbacks(singleton);

		// get SKUs
		GooglePlay_InAppPurchaseNative.base64Key = base64Key;
		skus = itemSKUs.split(":");
		types = itemTypes.split(":");
		skuList = new ArrayList<String>();
		for (int i = 0; i != skus.length; ++i) skuList.add(skus[i]);

		// init system
		ReignAndroidPlugin.RootActivity.runOnUiThread(new Runnable()
		{
			public void run()
			{
				helper = new IabHelper(ReignAndroidPlugin.RootActivity, base64Key);
				helper.enableDebugLogging(testing);
				helper.startSetup(new IabHelper.OnIabSetupFinishedListener()
				{
		            public void onIabSetupFinished(IabResult result)
		            {
		            	// we consume apps to reset the buy menu for testing
		            	if (result.isSuccess() && clearCache && testing)
		            	{
			            	try
			            	{
			            		helper.queryInventoryAsync(testResetInventoryListener);
			            	}
			            	catch (Exception e)
			            	{
			            		Log.d(logTag, "Reset Apps for testing Error: " + e.getMessage());
			            	}
		            	}
		            	else
		            	{
		            		initStatus = result.isSuccess() ? 1 : 2;
		            	}
		            }
		        });
			}
		});
	}

	public static int CheckInitStatus()
	{
		int status = initStatus;
		initStatus = 0;
		return status;
	}

	public static void Dispose()
	{
		ReignAndroidPlugin.RootActivity.runOnUiThread(new Runnable()
		{
			public void run()
			{
				if (helper != null) helper.dispose();
				helper = null;
			}
		});
	}

	// for clearing cache when testing the IAP plugin
	static IabHelper.QueryInventoryFinishedListener testResetInventoryListener = new IabHelper.QueryInventoryFinishedListener()
	{
        public void onQueryInventoryFinished(IabResult result, Inventory inventory)
        {
            if (result.isFailure())
            {
            	Log.d(logTag, "Test Consume Failed: " + result.getMessage());
            	initStatus = 1;
                return;
            }

            for (int i = 0; i != skus.length; ++i)
            {
	            Purchase premiumPurchase = inventory.getPurchase(skus[i]);
	            if (premiumPurchase != null) helper.consumeAsync(inventory.getPurchase(skus[i]), null);
            }

            initStatus = 1;
        }
    };

    public static void GetProductInfo()
	{
		productInfoItems = new ArrayList<String>();
		productInfoDone = false;

		ReignAndroidPlugin.RootActivity.runOnUiThread(new Runnable()
		{
			public void run()
			{
				helper.queryInventoryAsync(true, skuList, getProductInfoListener);
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

	static IabHelper.QueryInventoryFinishedListener getProductInfoListener = new IabHelper.QueryInventoryFinishedListener()
	{
        public void onQueryInventoryFinished(IabResult result, Inventory inventory)
        {
            if (result.isFailure())
            {
            	Log.d(logTag, "Get product info Failed: " + result.getMessage());
            	productInfoItems.clear();
            	productInfoDone = true;
                return;
            }

            for (int i = 0; i != skus.length; ++i)
            {
            	SkuDetails sku = inventory.getSkuDetails(skus[i]);
            	if (sku != null)
            	{
            		productInfoItems.add(sku.getSku());
            		productInfoItems.add(sku.getPrice());
            	}
            }
            productInfoDone = true;
        }
    };

	static IabHelper.QueryInventoryFinishedListener mGotInventoryListener = new IabHelper.QueryInventoryFinishedListener()
	{
        public void onQueryInventoryFinished(IabResult result, Inventory inventory)
        {
            if (result.isFailure())
            {
            	Log.d(logTag, "Restore Failed: " + result.getMessage());
                restoreDone = true;
                return;
            }

            for (int i = 0; i != skus.length; ++i)
            {
	            Purchase purchase = inventory.getPurchase(skus[i]);
	            if (purchase != null && Security.verifyPurchase(base64Key, purchase.getOriginalJson(), purchase.getSignature()))
            	{
	            	restoreItems.add(skus[i]);
            	}
            }

            restoreDone = true;
        }
    };

    public static void Restore()
    {
        restoreDone = false;
    	restoreItems = new ArrayList<String>();
		ReignAndroidPlugin.RootActivity.runOnUiThread(new Runnable()
		{
			public void run()
			{
				try
				{
					helper.queryInventoryAsync(mGotInventoryListener);
				}
				catch (Exception e)
				{
					Log.d(logTag, "Restore Apps Error: " + e.getMessage());
					restoreDone = true;
				}
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

    static IabHelper.OnIabPurchaseFinishedListener mPurchaseFinishedListener = new IabHelper.OnIabPurchaseFinishedListener()
    {
	    public void onIabPurchaseFinished(IabResult result, Purchase purchase)
	    {
	        if (result.isFailure())
	        {
	        	Log.d(logTag, "Buy Failed: " + result.getMessage());
	        	if (purchase != null) buySuccess = purchase.getPurchaseState() != IabHelper.BILLING_RESPONSE_RESULT_ITEM_ALREADY_OWNED;
	        	else buySuccess = false;
	            buyDone = true;
	            return;
	        }

	        buyReceipt = purchase.getOriginalJson();
	        if (purchase.getSku().equals(buyItem) && Security.verifyPurchase(base64Key, purchase.getOriginalJson(), purchase.getSignature()))
	        {
	        	boolean isConsumable = false;
	        	for (int i = 0; i != skus.length; ++i)
	        	{
	        		if (skus[i].equals(buyItem) && types[i].equals("Consumable"))
	        		{
	        			isConsumable = true;
	        			break;
	        		}
	        	}

	        	if (isConsumable)
	        	{
	        		Log.d(logTag, "Consumed: " + purchase.getSku());
	        		helper.consumeAsync(purchase, null);
	        	}

	            buySuccess = true;
	            buyDone = true;
	        }
	        else
	        {
	        	buySuccess = false;
	            buyDone = true;
	        }
	    }
	};

	public static void BuyApp(final String sku)
	{
		buyItem = sku;
		buyDone = false;
		buySuccess = false;
		ReignAndroidPlugin.RootActivity.runOnUiThread(new Runnable()
		{
			public void run()
			{
				try
				{
					helper.launchPurchaseFlow(ReignAndroidPlugin.RootActivity, sku, BuyIntentID, mPurchaseFinishedListener, "");
				}
				catch (Exception e)
				{
					Log.d(logTag, "Buy App Error: " + e.getMessage());
					buySuccess = false;
					buyDone = false;
				}
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
	
	@Override
	public boolean onActivityResult(int requestCode, int resultcode, Intent intent)
	{
		if (helper == null) return false;
        if (!helper.handleActivityResult(requestCode, resultcode, intent)) return false;
        
        return true;
	}
	
	@Override
	public void onPause()
	{
		// do nothing...
	}
	
	@Override
	public void onResume()
	{
		// do nothing...
	}
}
