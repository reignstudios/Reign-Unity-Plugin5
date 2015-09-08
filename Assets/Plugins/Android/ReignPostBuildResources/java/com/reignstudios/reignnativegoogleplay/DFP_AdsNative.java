package com.reignstudios.reignnativegoogleplay;

import java.util.ArrayList;
import java.util.List;

import com.google.android.gms.ads.*;
import com.google.android.gms.ads.doubleclick.PublisherAdRequest;
import com.google.android.gms.ads.doubleclick.PublisherAdView;

import com.reignstudios.reignnative.ReignAndroidPlugin;

import android.provider.Settings;
import android.util.Log;
import android.widget.RelativeLayout;

class DFP_AdsNative_Plugin
{
	public String ID;
	public PublisherAdView Ad;
	public AdSize Size;
	public boolean IsLoaded, Testing;

	public DFP_AdsNative_Plugin(String id, PublisherAdView ad, AdSize size, boolean isLoaded, boolean testing)
	{
		this.ID = id;
		this.Ad = ad;
		this.Size = size;
		this.IsLoaded = isLoaded;
		this.Testing = testing;
	}
}

public class DFP_AdsNative
{
	private static final String logTag = "Reign_DFP_BannerAds";
	private static AdMob_AdsNative Singleton;
	private static List<String> events;
	private static List<DFP_AdsNative_Plugin> ads;

	public static void CreateAd(final String unitID, final boolean testing, final boolean visible, final int gravity, final int adSize, final String adID)
	{
		if (Singleton == null) Singleton = new AdMob_AdsNative();
		if (events == null) events = new ArrayList<String>();
		if (ads == null) ads = new ArrayList<DFP_AdsNative_Plugin>();

		ReignAndroidPlugin.RootActivity.runOnUiThread(new Runnable()
		{
			public void run()
			{
				// find ad size
				AdSize currentAdSize = AdSize.BANNER;
				switch (adSize)
				{
					case 0: currentAdSize = AdSize.BANNER; break;
					case 1: currentAdSize = AdSize.SMART_BANNER; break;
					case 2: currentAdSize = AdSize.FULL_BANNER; break;
					case 3: currentAdSize = AdSize.LEADERBOARD; break;
					case 4: currentAdSize = AdSize.MEDIUM_RECTANGLE; break;
				}

				// create and place Ad
				PublisherAdView adView = new PublisherAdView(ReignAndroidPlugin.RootActivity);
				adView.setAdSizes(currentAdSize);
			    adView.setAdUnitId(unitID);
				adView.setVisibility(visible ? AdView.VISIBLE : AdView.GONE);

				// request Ad load
				PublisherAdRequest request = null;
				if (!testing)
				{
					request = new PublisherAdRequest.Builder().build();
				}
				else
				{
					String android_id = Settings.Secure.getString(ReignAndroidPlugin.RootActivity.getContentResolver(), Settings.Secure.ANDROID_ID);
					request = new PublisherAdRequest.Builder()
					.addTestDevice(PublisherAdRequest.DEVICE_ID_EMULATOR)
			        .addTestDevice(Utils.MD5(android_id).toUpperCase())
			        .build();
				}

				ads.add(new DFP_AdsNative_Plugin(adID, adView, currentAdSize, false, testing));
				events.add("Created:" + adID);

				adView.setAdListener(new AdListener()
				{
					@Override
					public void onAdLoaded()
					{
						for (DFP_AdsNative_Plugin ad : ads)
						{
							if (!ad.IsLoaded)
							{
								ad.IsLoaded = true;
								RelativeLayout.LayoutParams adViewParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WRAP_CONTENT, RelativeLayout.LayoutParams.WRAP_CONTENT);
								ReignAndroidPlugin.ContentView.addView(ad.Ad, adViewParams);
								setGravity(ad.Ad, gravity);
							}
						}

						Log.d(logTag, "Refreshed");
						events.add("Refreshed");
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
						Log.d(logTag, "Clicked");
						events.add("Clicked");
					}

					@Override
					public void onAdClosed()
					{
						// do nothing...
					}

					@Override
					public void onAdLeftApplication()
					{
						// do nothing...
					}
				});

				adView.loadAd(request);
			}
		});
	}

	private static DFP_AdsNative_Plugin findAd(String adID)
	{
		for (DFP_AdsNative_Plugin ad : ads)
		{
			if (ad.ID.equals(adID)) return ad;
		}

		Log.d(logTag, "Failed to find AdID: " + adID);
		return null;
	}

	public static void Dispose(final String adID)
	{
		ReignAndroidPlugin.RootActivity.runOnUiThread(new Runnable()
		{
			public void run()
			{
				DFP_AdsNative_Plugin ad = findAd(adID);
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
				DFP_AdsNative_Plugin ad = findAd(adID);
				PublisherAdRequest request = null;
				if (!ad.Testing)
				{
					request = new PublisherAdRequest.Builder().build();
				}
				else
				{
					String android_id = Settings.Secure.getString(ReignAndroidPlugin.RootActivity.getContentResolver(), Settings.Secure.ANDROID_ID);
					request = new PublisherAdRequest.Builder()
					.addTestDevice(PublisherAdRequest.DEVICE_ID_EMULATOR)
			        .addTestDevice(Utils.MD5(android_id).toUpperCase())
			        .build();
				}

				ad.Ad.loadAd(request);
			}
		});
	}

	public static void SetVisible(final String adID, final boolean visible)
	{
		ReignAndroidPlugin.RootActivity.runOnUiThread(new Runnable()
		{
			public void run()
			{
				PublisherAdView adView = findAd(adID).Ad;
				adView.setVisibility(visible ? AdView.VISIBLE : AdView.GONE);
			}
		});
	}

	public static void SetGravity(final String adID, final int gravity)
	{
		ReignAndroidPlugin.RootActivity.runOnUiThread(new Runnable()
		{
			public void run()
			{
				PublisherAdView adView = findAd(adID).Ad;
				setGravity(adView, gravity);
			}
		});
	}
	
	private static void setGravity(PublisherAdView adView, int gravity)
	{
		RelativeLayout.LayoutParams viewParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WRAP_CONTENT, RelativeLayout.LayoutParams.WRAP_CONTENT);
		switch (gravity)
		{
			case 0:
				viewParams.addRule(RelativeLayout.ALIGN_PARENT_BOTTOM);
				viewParams.addRule(RelativeLayout.ALIGN_LEFT);
				break;
				
			case 1:
				viewParams.addRule(RelativeLayout.ALIGN_PARENT_BOTTOM);
				viewParams.addRule(RelativeLayout.ALIGN_RIGHT);
				break;
				
			case 2:
				viewParams.addRule(RelativeLayout.ALIGN_PARENT_BOTTOM);
				viewParams.addRule(RelativeLayout.CENTER_HORIZONTAL);
				break;
				
			case 3:
				viewParams.addRule(RelativeLayout.ALIGN_PARENT_TOP);
				viewParams.addRule(RelativeLayout.ALIGN_LEFT);
				break;
				
			case 4:
				viewParams.addRule(RelativeLayout.ALIGN_PARENT_TOP);
				viewParams.addRule(RelativeLayout.ALIGN_RIGHT);
				break;
				
			case 5:
				viewParams.addRule(RelativeLayout.ALIGN_PARENT_TOP);
				viewParams.addRule(RelativeLayout.CENTER_HORIZONTAL);
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
}
