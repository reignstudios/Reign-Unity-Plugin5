package com.reignstudios.reignnative;

import java.util.ArrayList;
import java.util.List;

import android.provider.Settings;
import android.util.Log;

import com.amazon.device.ads.*;

class Amazon_InterstitialAdsNative_Plugin
{
	public String ID, AppKey;
	public InterstitialAd Ad;
	public boolean Testing;
	
	public Amazon_InterstitialAdsNative_Plugin(String id, String appKey, boolean testing)
	{
		this.ID = id;
		this.AppKey = appKey;
		this.Ad = null;
		this.Testing = testing;
	}
}

public class Amazon_InterstitialAdNative
{
	private static final String logTag = "Reign_Amazon_InterstitialAds";
	private static List<String> events;
	private static List<Amazon_InterstitialAdsNative_Plugin> ads;
	
	public static void CreateAd(final String appKey, final boolean testing, final String adID)
	{
		if (events == null) events = new ArrayList<String>();
		if (ads == null) ads = new ArrayList<Amazon_InterstitialAdsNative_Plugin>();
		
		 AdRegistration.enableLogging(testing);
		 AdRegistration.enableTesting(testing);
		
		ads.add(new Amazon_InterstitialAdsNative_Plugin(adID, appKey, testing));
		events.add("Created:" + adID);
	}
	
	private static Amazon_InterstitialAdsNative_Plugin findAd(String adID)
	{
		for (Amazon_InterstitialAdsNative_Plugin ad : ads)
		{
			if (ad.ID.equals(adID)) return ad;
		}
		
		Log.d(logTag, "Failed to find AdID: " + adID);
		return null;
	}
	
	public static void Dispose(final String adID)
	{
		ReignUnityActivity.ReignContext.runOnUiThread(new Runnable()
		{
			public void run()
			{
				Amazon_InterstitialAdsNative_Plugin ad = findAd(adID);
				ads.remove(ad);
			}
		});
	}
	
	public static void Cache(final String adID)
	{
		ReignUnityActivity.ReignContext.runOnUiThread(new Runnable()
		{
			public void run()
			{
				Amazon_InterstitialAdsNative_Plugin ad = findAd(adID);
				InterstitialAd iAd = new InterstitialAd(ReignUnityActivity.ReignContext);
				AdRegistration.setAppKey(ad.AppKey);
				iAd.setListener(new DefaultAdListener() {
			        @Override
			        public void onAdLoaded(final Ad ad, final AdProperties adProperties)
			        {
			            Log.d(logTag, "Loaded: " + adProperties.getAdType().toString());
						events.add("Cached");
			        }
			    
			        @Override
			        public void onAdFailedToLoad(final Ad view, final AdError error)
			        {
			            Log.d(logTag, "Error: " + error.getMessage());
						events.add("Error:" + error.getCode());
			        }
			        
			        @Override
			        public void onAdDismissed(final Ad ad)
			        {
			            Log.d(logTag, "Canceled");
						events.add("Canceled");
			        }
		        });
				
				ad.Ad = iAd;
				iAd.loadAd();
			}
		});
	}
	
	public static void Show(final String adID)
	{
		ReignUnityActivity.ReignContext.runOnUiThread(new Runnable()
		{
			public void run()
			{
				events.add("Shown");
				Amazon_InterstitialAdsNative_Plugin ad = findAd(adID);
				if (!ad.Ad.isLoading()) ad.Ad.showAd();
			}
		});
	}
	
	public static boolean HasEvents()
	{
		return events.size() != 0;
	}
	
	public static String GetNextEvent()
	{
		int index = events.size()-1;
		String next = events.get(index);
		events.remove(index);
		return next;
	}
}
