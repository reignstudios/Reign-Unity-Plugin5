package com.reignstudios.reignnative;

import java.util.Date;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.List;
import java.util.Locale;

import android.util.Log;

import com.sec.android.iap.lib.helper.SamsungIapHelper;
import com.sec.android.iap.lib.listener.OnGetInboxListener;
import com.sec.android.iap.lib.listener.OnGetItemListener;
import com.sec.android.iap.lib.listener.OnPaymentListener;
import com.sec.android.iap.lib.vo.ErrorVo;
import com.sec.android.iap.lib.vo.InboxVo;
import com.sec.android.iap.lib.vo.ItemVo;
import com.sec.android.iap.lib.vo.PurchaseVo;

public class Samsung_InAppPurchaseNative implements OnPaymentListener, OnGetInboxListener, OnGetItemListener
{
	private static final String logTag = "Reign_Samsung_IAP";
	private static Samsung_InAppPurchaseNative singleton;
	private static boolean testing;
	private static SamsungIapHelper helper;
	private static int initStatus;
	//private static String[] skus, types;
	private static boolean restoreDone, buyDone, buySuccess, productInfoDone;
	private static List<String> restoreItems, productInfoItems;
	private static String itemGroupID, buyReceipt;
	
	public static void Init(final String itemGroupID, String itemSKUs, String itemTypes, final boolean testing)
	{
		Samsung_InAppPurchaseNative.testing = testing;
		Samsung_InAppPurchaseNative.itemGroupID = itemGroupID;
		if (singleton == null) singleton = new Samsung_InAppPurchaseNative();
		//skus = itemSKUs.split(":");
		//types = itemTypes.split(":");
		
		ReignUnityActivity.ReignContext.runOnUiThread(new Runnable()
		{
			public void run()
			{
				try
				{
					int mode = testing ? SamsungIapHelper.IAP_MODE_TEST_SUCCESS : SamsungIapHelper.IAP_MODE_COMMERCIAL;
					helper = SamsungIapHelper.getInstance(ReignUnityActivity.ReignContext, mode);
					initStatus = 1;
				}
				catch (Exception e)
				{
					Log.d(logTag, "Init Error: " + e.getMessage());
					initStatus = 2;
				}
			}
		});
	}
	
	public static int CheckInitStatus()
	{
		int status = initStatus;
		initStatus = 0;
		return status;
	}
	
	@Override
	public void onGetItem(ErrorVo _errorVo, ArrayList<ItemVo> _itemList)
	{
		if(_errorVo != null && _errorVo.getErrorCode() == SamsungIapHelper.IAP_ERROR_NONE)
        {
            if(_itemList != null && _itemList.size() > 0)
            {
            	for (int i = 0; i != _itemList.size(); ++i)
            	{
            		ItemVo item = _itemList.get(i);
            		productInfoItems.add(item.getItemId());
            		productInfoItems.add(item.getItemPriceString());
            	}
            	productInfoDone = true;
            }
        }
		else if (_errorVo.getErrorCode() != SamsungIapHelper.IAP_ERROR_NONE)
		{
			Log.e(logTag, "GetProduct Error: " + _errorVo.getErrorString());
			productInfoDone = true;
		}
		else
		{
			Log.e(logTag, "GetProduct Error: Unknown");
			productInfoDone = true;
		}
	}
	
	public static void GetProductInfo()
	{
		productInfoItems = new ArrayList<String>();
		productInfoDone = false;
		ReignUnityActivity.ReignContext.runOnUiThread(new Runnable()
		{
			public void run()
			{
                int mode = testing ? SamsungIapHelper.IAP_MODE_TEST_SUCCESS : SamsungIapHelper.IAP_MODE_COMMERCIAL;
				helper.getItemList(itemGroupID, 0, 100, SamsungIapHelper.ITEM_TYPE_ALL, mode, singleton);
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
	
	@Override
	public void onGetItemInbox(ErrorVo _errorVo, ArrayList<InboxVo> _inboxList)
	{
		if(_errorVo != null && _errorVo.getErrorCode() == SamsungIapHelper.IAP_ERROR_NONE)
        {
            if(_inboxList != null && _inboxList.size() > 0)
            {
            	for (int i = 0; i != _inboxList.size(); ++i)
            	{
            		InboxVo item = _inboxList.get(i);
            		restoreItems.add(item.getItemId());
            	}
            	restoreDone = true;
            }
        }
		else if (_errorVo.getErrorCode() != SamsungIapHelper.IAP_ERROR_NONE)
		{
			Log.e(logTag, "Restore Error: " + _errorVo.getErrorString());
			restoreDone = true;
		}
		else
		{
			Log.e(logTag, "Restore Error: Unknown");
			restoreDone = true;
		}
	}
	
	public static void Restore()
    {
        restoreDone = false;
    	restoreItems = new ArrayList<String>();
    	ReignUnityActivity.ReignContext.runOnUiThread(new Runnable()
		{
			public void run()
			{
				Date d = new Date();
                SimpleDateFormat sdf = new SimpleDateFormat("yyyyMMdd", Locale.getDefault());
                String today = sdf.format(d);
				helper.getItemInboxList(itemGroupID, 0, 100, "20110101", today, singleton);
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

    @Override
    public void onPayment(ErrorVo _errorVo, PurchaseVo _purchaseVo)
    {
        if(_errorVo != null && _errorVo.getErrorCode() == SamsungIapHelper.IAP_ERROR_NONE)
        {
        	buyReceipt = _purchaseVo.getJsonString();
        	buySuccess = true;
        	buyDone = true;
        }
        else if (_errorVo.getErrorCode() != SamsungIapHelper.IAP_ERROR_NONE)
        {
        	Log.e(logTag, "Buy Error: " + _errorVo.getErrorString());
        	buySuccess = false;
        	buyDone = true;
        }
        else
        {
        	Log.e(logTag, "Buy Error: Unknown");
        	buySuccess = false;
        	buyDone = true;
        }
        
        // Test Code
        if (testing)
        {
	        if( _errorVo != null )
	        {
	            Log.e(logTag, _errorVo.dump());
	        }
	        
	        if( _purchaseVo != null )
	        {
	            Log.e(logTag, _purchaseVo.dump());
	        }
        }
    }
	
	public static void BuyApp(final String sku)
	{
		buyDone = false;
		buySuccess = false;
		ReignUnityActivity.ReignContext.runOnUiThread(new Runnable()
		{
			public void run()
			{
				helper.startPayment(itemGroupID, sku, true, singleton);
			}
		});
	}
	
	public static boolean CheckBuyDone()
    {
		boolean done = buyDone;
		if (buyDone) buyDone = false;
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
}
