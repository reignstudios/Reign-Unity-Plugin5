package com.reignstudios.reignnative;

import java.util.ArrayList;
import java.util.List;

import android.provider.Settings;
import android.util.Log;

import com.google.android.gms.ads.*;
import com.google.android.gms.ads.doubleclick.PublisherAdRequest;
import com.google.android.gms.ads.doubleclick.PublisherInterstitialAd;

class DFP_InterstitialAdsNative_Plugin
{
	public String ID, UnitID;
	public PublisherInterstitialAd Ad;
	public boolean Testing;
	
	public DFP_InterstitialAdsNative_Plugin(String id, String unitID, boolean testing)
	{
		this.ID = id;
		this.UnitID = unitID;
		this.Ad = null;
		this.Testing = testing;
	}
}

public class DFP_InterstitialAdNative
{
	private static final String logTag = "Reign_DFP_InterstitialAds";
	private static List<String> events;
	private static List<DFP_InterstitialAdsNative_Plugin> ads;
	
	public static void CreateAd(final String unitID, final boolean testing, final String adID)
	{
		if (events == null) events = new ArrayList<String>();
		if (ads == null) ads = new ArrayList<DFP_InterstitialAdsNative_Plugin>();
		
		ads.add(new DFP_InterstitialAdsNative_Plugin(adID, unitID, testing));
		events.add("Created:" + adID);
	}
	
	private static DFP_InterstitialAdsNative_Plugin findAd(String adID)
	{
		for (DFP_InterstitialAdsNative_Plugin ad : ads)
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
				DFP_InterstitialAdsNative_Plugin ad = findAd(adID);
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
				DFP_InterstitialAdsNative_Plugin ad = findAd(adID);
				PublisherInterstitialAd iAd = new PublisherInterstitialAd(ReignUnityActivity.ReignContext);
				iAd.setAdUnitId(ad.UnitID);
				iAd.setAdListener(new AdListener() {
					@Override
					public void onAdLoaded()
					{
						Log.d(logTag, "Loaded");
						events.add("Cached");
					}
					
					@Override
					public void onAdFailedToLoad(int errorCode)
					{
						Log.d(logTag, "Error: " + errorCode);
						events.add("Error:" + errorCode);
					}
					
					@Override
					public void onAdOpened()
					{
						// do nothing...
					}
					
					@Override
					public void onAdClosed()
					{
						Log.d(logTag, "Canceled");
						events.add("Canceled");
					}
					
					@Override
					public void onAdLeftApplication()
					{
						Log.d(logTag, "Clicked");
						events.add("Clicked");
					}
		        });
				
				ad.Ad = iAd;
				PublisherAdRequest request = null;
				if (ad.Testing)
				{
					String android_id = Settings.Secure.getString(ReignUnityActivity.ReignContext.getContentResolver(), Settings.Secure.ANDROID_ID);
					request = new PublisherAdRequest.Builder()
					.addTestDevice(PublisherAdRequest.DEVICE_ID_EMULATOR)
			        .addTestDevice(Utils.MD5(android_id).toUpperCase())
			        .build();
				}
				else
				{
					request = new PublisherAdRequest.Builder().build();
				}
				
				iAd.loadAd(request);
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
				DFP_InterstitialAdsNative_Plugin ad = findAd(adID);
				if (ad.Ad.isLoaded()) ad.Ad.show();
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
