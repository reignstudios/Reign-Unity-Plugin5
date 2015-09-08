package com.reignstudios.reignnativeamazon;

import java.util.ArrayList;
import java.util.List;
import android.util.Log;
import android.widget.RelativeLayout;
import com.amazon.device.ads.*;
import com.reignstudios.reignnative.ReignAndroidPlugin;

class Amazon_AdsNative_Plugin
{
	public String ID, AppKey;
	public AdLayout Ad;
	public AdSize Size;
	public boolean Testing;
	
	public Amazon_AdsNative_Plugin(String id, String appKey, AdLayout ad, AdSize size, boolean testing)
	{
		this.ID = id;
		this.AppKey = appKey;
		this.Ad = ad;
		this.Size = size;
		this.Testing = testing;
	}
}

public class Amazon_AdsNative implements AdListener
{
	private static final String logTag = "Reign_Amazon_BannerAds";
	private static Amazon_AdsNative Singleton;
	private static List<String> events;
	private static List<Amazon_AdsNative_Plugin> ads;
	
	private static boolean refreshThreadRunning;
	private static Thread refreshThread;
	
	public static void CreateAd(final String appKey, final boolean testing, final boolean visible, final int gravity, final int adSize, final int refreshSec, final String adID)
	{
		if (Singleton == null) Singleton = new Amazon_AdsNative();
		if (events == null) events = new ArrayList<String>();
		if (ads == null) ads = new ArrayList<Amazon_AdsNative_Plugin>();

		ReignAndroidPlugin.RootActivity.runOnUiThread(new Runnable()
		{
			public void run()
			{
				AdRegistration.enableLogging(testing);
				AdRegistration.enableTesting(testing);
				
				// find ad size
				AdSize currentAdSize = AdSize.SIZE_320x50;
				switch (adSize)
				{
					case 0: currentAdSize = AdSize.SIZE_300x50; break;
					case 1: currentAdSize = AdSize.SIZE_300x250; break;
					case 2: currentAdSize = AdSize.SIZE_320x50; break;
					case 3: currentAdSize = AdSize.SIZE_600x90; break;
					case 4: currentAdSize = AdSize.SIZE_728x90; break;
					case 5: currentAdSize = AdSize.SIZE_1024x50; break;
				}
				
				AdLayout adView = new AdLayout(ReignAndroidPlugin.RootActivity, currentAdSize);// <<<<<<<< NEED TO ADD THESE
				adView.setListener(Singleton);
				adView.setVisibility(visible ? AdLayout.VISIBLE : AdLayout.GONE);
				RelativeLayout.LayoutParams adViewParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WRAP_CONTENT, RelativeLayout.LayoutParams.WRAP_CONTENT);
				ReignAndroidPlugin.ContentView.addView(adView, adViewParams);
				setGravity(adView, gravity);
				
				try
				{
					String currentAppKey = testing ? "sample-app-v1_pub-2" : appKey;
		            AdRegistration.setAppKey(currentAppKey);
		            AdTargetingOptions adOptions = new AdTargetingOptions();
		            adView.loadAd(adOptions);
		            Amazon_AdsNative_Plugin ad = new Amazon_AdsNative_Plugin(adID, currentAppKey, adView, currentAdSize, testing);
		            ads.add(ad);
		            events.add("Created:" + ad.ID);
		        }
				catch (Exception e)
				{
		            Log.e(logTag, "Exception thrown: " + e.toString());
		            events.add("Created:Failed");
		        }
			}
		});
		
		if (refreshThread == null)
		{
			refreshThread = new Thread()
	        {
	            @Override
	            public void run()
	            {
	            	int refreshTime = 1000 * refreshSec;
	            	refreshThreadRunning = true;
	            	while (refreshThreadRunning)
	            	{
		                try
		                {
		                	synchronized(Singleton)
		                	{
		                		Singleton.wait(refreshTime);
								ReignAndroidPlugin.RootActivity.runOnUiThread(new Runnable()
		                		{
		                			public void run()
		                			{
		                				for (int i = 0; i != ads.size(); ++i)
		                				{
		                					Amazon_AdsNative_Plugin ad = ads.get(i);
			                				AdRegistration.setAppKey(ad.AppKey);
			            		            AdTargetingOptions adOptions = new AdTargetingOptions();
			            		            ad.Ad.loadAd(adOptions);
		                				}
		                			}
		                		});
		                	}
		                }
		                catch(InterruptedException ex)
		                {     
		                	Log.e(logTag, "Refresh failed: " + ex.getMessage());
		                }
	            	}
	            }
	        };
	        
	        refreshThread.start();
		}
	}
	
	private static Amazon_AdsNative_Plugin findAd(String adID)
	{
		for (Amazon_AdsNative_Plugin ad : ads)
		{
			if (ad.ID.equals(adID)) return ad;
		}
		
		Log.d(logTag, "Failed to find AdID: " + adID);
		return null;
	}
	
	public static void Dispose(final String adID)
	{
		if (refreshThread != null)
		{
			refreshThreadRunning = false;
			refreshThread = null;
		}

		ReignAndroidPlugin.RootActivity.runOnUiThread(new Runnable()
		{
			public void run()
			{
				Amazon_AdsNative_Plugin ad = findAd(adID);
				ads.remove(ad);
				
				ReignAndroidPlugin.ContentView.removeView(ad.Ad);
				ad.Ad.destroy();
			}
		});
	}
	
	public static void Refresh(final String adID)
	{
		ReignAndroidPlugin.RootActivity.runOnUiThread(new Runnable()
		{
			public void run()
			{
				Amazon_AdsNative_Plugin ad = findAd(adID);
				AdRegistration.setAppKey(ad.AppKey);
	            AdTargetingOptions adOptions = new AdTargetingOptions();
	            ad.Ad.loadAd(adOptions);
			}
		});
	}
	
	public static void SetVisible(final String adID, final boolean visible)
	{
		ReignAndroidPlugin.RootActivity.runOnUiThread(new Runnable()
		{
			public void run()
			{
				AdLayout adView = findAd(adID).Ad;
				adView.setVisibility(visible ? AdLayout.VISIBLE : AdLayout.GONE);
			}
		});
	}
	
	public static void SetGravity(final String adID, final int gravity)
	{
		ReignAndroidPlugin.RootActivity.runOnUiThread(new Runnable()
		{
			public void run()
			{
				AdLayout adView = findAd(adID).Ad;
				setGravity(adView, gravity);
			}
		});
	}
	
	private static void setGravity(AdLayout adView, int gravity)
	{
		RelativeLayout.LayoutParams viewParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WRAP_CONTENT, RelativeLayout.LayoutParams.WRAP_CONTENT);
		switch (gravity)
		{
			case 0:
				viewParams.addRule(RelativeLayout.ALIGN_PARENT_BOTTOM);
				viewParams.addRule(RelativeLayout.ALIGN_PARENT_LEFT);
				break;
				
			case 1:
				viewParams.addRule(RelativeLayout.ALIGN_PARENT_BOTTOM);
				viewParams.addRule(RelativeLayout.ALIGN_PARENT_RIGHT);
				break;
				
			case 2:
				viewParams.addRule(RelativeLayout.ALIGN_PARENT_BOTTOM);
				viewParams.addRule(RelativeLayout.CENTER_HORIZONTAL);
				break;
				
			case 3:
				viewParams.addRule(RelativeLayout.ALIGN_PARENT_TOP);
				viewParams.addRule(RelativeLayout.ALIGN_PARENT_LEFT);
				break;
				
			case 4:
				viewParams.addRule(RelativeLayout.ALIGN_PARENT_TOP);
				viewParams.addRule(RelativeLayout.ALIGN_PARENT_RIGHT);
				break;
				
			case 5:
				viewParams.addRule(RelativeLayout.ALIGN_PARENT_TOP);
				viewParams.addRule(RelativeLayout.CENTER_HORIZONTAL);
				break;
				
			case 6:
				viewParams.addRule(RelativeLayout.CENTER_HORIZONTAL);
				viewParams.addRule(RelativeLayout.CENTER_VERTICAL);
				break;
		}
		
		adView.setLayoutParams(viewParams);
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

	@Override
	public void onAdCollapsed(Ad arg0)
	{
		Log.d(logTag, "Ad collapsed.");
	}

	@Override
	public void onAdDismissed(Ad arg0)
	{
		Log.d(logTag, "Ad dismissed.");
	}

	@Override
	public void onAdExpanded(Ad arg0)
	{
		Log.d(logTag, "Ad expanded.");
        events.add("Clicked");
	}

	@Override
	public void onAdFailedToLoad(Ad arg0, AdError error)
	{
		Log.w(logTag, "Ad failed to load. Code: " + error.getCode() + ", Message: " + error.getMessage());
        events.add("Error:" + error.getMessage());
	}

	@Override
	public void onAdLoaded(Ad arg0, AdProperties adProperties)
	{
		Log.d(logTag, adProperties.getAdType().toString() + " Ad loaded successfully.");
        events.add("Refreshed");
	}
}
