#if REIGN_POSTBUILD
using System;
using GoogleAds;
using UnityEngine;

namespace Reign.Plugin
{
	public class AdMob_InterstitialAdPlugin_Native : IInterstitialAdPlugin
	{
		private string unitID;
		private GoogleAds.InterstitialAd ad;
		private InterstitialAdEventCallbackMethod eventCallback;
		private bool testing;

		public AdMob_InterstitialAdPlugin_Native(InterstitialAdDesc desc, InterstitialAdCreatedCallbackMethod createdCallback)
		{
			bool pass = true;
			try
			{
				eventCallback = desc.EventCallback;
				unitID = desc.WP8_AdMob_UnitID;
				testing = desc.Testing;
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message);
				pass = false;
			}

			if (createdCallback != null) createdCallback(pass);
		}

		void ad_LeavingApplication(object sender, AdEventArgs e)
		{
			ReignServices.InvokeOnUnityThread(delegate
			{
				if (eventCallback != null) eventCallback(InterstitialAdEvents.Clicked, null);
			});
		}

		void ad_DismissingOverlay(object sender, AdEventArgs e)
		{
			ReignServices.InvokeOnUnityThread(delegate
			{
				if (eventCallback != null) eventCallback(InterstitialAdEvents.Canceled, null);
			});
		}

		void ad_ShowingOverlay(object sender, AdEventArgs e)
		{
			ReignServices.InvokeOnUnityThread(delegate
			{
				if (eventCallback != null) eventCallback(InterstitialAdEvents.Shown, null);
			});
		}

		void ad_FailedToReceiveAd(object sender, AdErrorEventArgs e)
		{
			ReignServices.InvokeOnUnityThread(delegate
			{
				if (eventCallback != null) eventCallback(InterstitialAdEvents.Error, "Error Code: " + e.ErrorCode);
			});
		}

		private void adReceived(object sender, AdEventArgs e)
		{
			ReignServices.InvokeOnUnityThread(delegate
			{
				if (eventCallback != null) eventCallback(InterstitialAdEvents.Cached, null);
			});
		}

		public void Cache()
		{
			WinRTPlugin.Dispatcher.BeginInvoke(delegate()
			{
				ad = new GoogleAds.InterstitialAd(unitID);
				ad.ReceivedAd += adReceived;
				ad.FailedToReceiveAd += ad_FailedToReceiveAd;
				ad.ShowingOverlay += ad_ShowingOverlay;
				ad.DismissingOverlay += ad_DismissingOverlay;
				ad.LeavingApplication += ad_LeavingApplication;

				var adRequest = new AdRequest();
				adRequest.ForceTesting = testing;
				ad.LoadAd(adRequest);
			});
		}

		public void Show()
		{
			WinRTPlugin.Dispatcher.BeginInvoke(delegate()
			{
				ad.ShowAd();
			});
		}

		public void Dispose()
		{
			if (ad != null)
			{
				ad = null;
			}
		}

		public void Update()
		{
			// do nothing...
		}

		public void OnGUI()
		{
			// do nothing...
		}

		public void OverrideOnGUI()
		{
			// do nothing...
		}
	}
}
#elif UNITY_WP8
namespace Reign.Plugin
{
	public class AdMob_InterstitialAdPlugin_WP8 : IInterstitialAdPlugin
	{
		public IInterstitialAdPlugin Native;

		public delegate void InitNativeMethod(AdMob_InterstitialAdPlugin_WP8 plugin, InterstitialAdDesc desc, InterstitialAdCreatedCallbackMethod createdCallback);
		public static InitNativeMethod InitNative;

		public AdMob_InterstitialAdPlugin_WP8(InterstitialAdDesc desc, InterstitialAdCreatedCallbackMethod createdCallback)
		{
			InitNative(this, desc, createdCallback);
		}

		public void Cache()
		{
			Native.Cache();
		}

		public void Show()
		{
			Native.Show();
		}

		public void Dispose()
		{
			Native.Dispose();
		}

		public void Update()
		{
			Native.Update();
		}

		public void OnGUI()
		{
			Native.OnGUI();
		}

		public void OverrideOnGUI()
		{
			Native.OverrideOnGUI();
		}
	}
}
#endif