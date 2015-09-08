package com.reignstudios.reignnativegoogleplay;

import java.util.ArrayList;
import java.util.List;

import com.google.android.gms.ads.*;

import com.reignstudios.reignnative.ReignAndroidPlugin;

import android.provider.Settings;
import android.util.Log;
import android.widget.RelativeLayout;

class AdMob_AdsNative_Plugin
{
	public String ID;
	public AdView Ad;
	public AdSize Size;
	public boolean IsLoaded, Testing;

	public AdMob_AdsNative_Plugin(String id, AdView ad, AdSize size, boolean isLoaded, boolean testing)
	{
		this.ID = id;
		this.Ad = ad;
		this.Size = size;
		this.IsLoaded = isLoaded;
		this.Testing = testing;
	}
}

public class AdMob_AdsNative
{
	private static final String logTag = "Reign_AdMob_BannerAds";
	private static AdMob_AdsNative Singleton;
	private static List<String> events;
	private static List<AdMob_AdsNative_Plugin> ads;

	public static void CreateAd(final String unitID, final boolean testing, final boolean visible, final int gravity, final int adSize, final String adID)
	{
		if (Singleton == null) Singleton = new AdMob_AdsNative();
		if (events == null) events = new ArrayList<String>();
		if (ads == null) ads = new ArrayList<AdMob_AdsNative_Plugin>();

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
				AdView adView = new AdView(ReignAndroidPlugin.RootActivity);
				adView.setAdSize(currentAdSize);
			    adView.setAdUnitId(unitID);
				adView.setVisibility(visible ? AdView.VISIBLE : AdView.GONE);

				// request Ad load
				AdRequest request = null;
				if (!testing)
				{
					request = new AdRequest.Builder().build();
				}
				else
				{
					String android_id = Settings.Secure.getString(ReignAndroidPlugin.RootActivity.getContentResolver(), Settings.Secure.ANDROID_ID);
					request = new AdRequest.Builder()
					.addTestDevice(AdRequest.DEVICE_ID_EMULATOR)
			        .addTestDevice(Utils.MD5(android_id).toUpperCase())
			        .build();
				}

				ads.add(new AdMob_AdsNative_Plugin(adID, adView, currentAdSize, false, testing));
				events.add("Created:" + adID);

				adView.setAdListener(new AdListener()
				{
					@Override
					public void onAdLoaded()
					{
						for (AdMob_AdsNative_Plugin ad : ads)
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

	private static AdMob_AdsNative_Plugin findAd(String adID)
	{
		for (AdMob_AdsNative_Plugin ad : ads)
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
				AdMob_AdsNative_Plugin ad = findAd(adID);
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
				AdMob_AdsNative_Plugin ad = findAd(adID);
				AdRequest request = null;
				if (!ad.Testing)
				{
					request = new AdRequest.Builder().build();
				}
				else
				{
					String android_id = Settings.Secure.getString(ReignAndroidPlugin.RootActivity.getContentResolver(), Settings.Secure.ANDROID_ID);
					request = new AdRequest.Builder()
					.addTestDevice(AdRequest.DEVICE_ID_EMULATOR)
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
				AdView adView = findAd(adID).Ad;
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
				AdView adView = findAd(adID).Ad;
				setGravity(adView, gravity);
			}
		});
	}
	
	private static void setGravity(AdView adView, int gravity)
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
}
