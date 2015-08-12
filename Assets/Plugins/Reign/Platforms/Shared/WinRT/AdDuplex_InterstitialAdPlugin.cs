#if REIGN_POSTBUILD && UNITY_WP_8_1
using System;
using UnityEngine;
using Windows.UI.Core;

using AdDuplex.Universal.Controls.WinPhone.XAML;
using AdDuplex.Universal.WinPhone.WinRT.Models;
using AdDuplex.Universal.WinPhone.WinRT.Core;

namespace Reign.Plugin
{
	public class AdDuplex_InterstitialAdPlugin_Native : IInterstitialAdPlugin
	{
		private string unitID;
		private AdDuplex.Universal.Controls.WinPhone.XAML.InterstitialAd ad;
		private InterstitialAdEventCallbackMethod eventCallback;
		private bool testing;
		private static bool initialized;

		public AdDuplex_InterstitialAdPlugin_Native(InterstitialAdDesc desc, InterstitialAdCreatedCallbackMethod createdCallback)
		{
			init(desc, createdCallback);
		}

		private async void init(InterstitialAdDesc desc, InterstitialAdCreatedCallbackMethod createdCallback)
		{
			bool pass = true;
			try
			{
				eventCallback = desc.EventCallback;
				unitID = desc.WP8_AdDuplex_UnitID;
				testing = desc.Testing;
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message);
				pass = false;
			}

			if (!initialized && pass)
			{
				initialized = true;
				await WinRTPlugin.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate()
				{
					AdDuplexClient.Initialize(desc.WP8_AdDuplex_ApplicationKey);
					ReignServices.InvokeOnUnityThread(delegate
					{
						if (createdCallback != null) createdCallback(pass);
					});
				});
			}
			else
			{
				if (createdCallback != null) createdCallback(pass);
			}
		}

		private void ad_AdClicked(object sender, InterstitialAdClickEventArgs e)
		{
			ReignServices.InvokeOnUnityThread(delegate
			{
				if (eventCallback != null) eventCallback(InterstitialAdEvents.Clicked, null);
			});
		}

		private void ad_AdClosed(object sender, InterstitialAdLoadedEventArgs e)
		{
			ReignServices.InvokeOnUnityThread(delegate
			{
				if (eventCallback != null) eventCallback(InterstitialAdEvents.Canceled, null);
			});
		}

		private void ad_ShowingOverlay()
		{
			ReignServices.InvokeOnUnityThread(delegate
			{
				if (eventCallback != null) eventCallback(InterstitialAdEvents.Shown, null);
			});
		}

		private void ad_AdLoadingError(object sender, InterstitialAdLoadingErrorEventArgs e)
		{
			ReignServices.InvokeOnUnityThread(delegate
			{
				if (eventCallback != null) eventCallback(InterstitialAdEvents.Error, "Error: " + e.Error.Message);
			});
		}

		private void ad_NoAd(object sender, NoAdEventArgs e)
		{
			ReignServices.InvokeOnUnityThread(delegate
			{
				if (eventCallback != null) eventCallback(InterstitialAdEvents.Error, "NoAd Error: " + e.Message);
			});
		}

		private void ad_AdLoaded(object sender, InterstitialAdLoadedEventArgs e)
		{
			ReignServices.InvokeOnUnityThread(delegate
			{
				if (eventCallback != null) eventCallback(InterstitialAdEvents.Cached, null);
			});
		}

		public async void Cache()
		{
			await WinRTPlugin.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate()
			{
				ad = new AdDuplex.Universal.Controls.WinPhone.XAML.InterstitialAd(unitID);
				//ad.AdLoaded += ad_AdLoaded;// never fires this, so we manually call it below
				ad.AdLoadingError += ad_AdLoadingError;
				ad.NoAd += ad_NoAd;
				ad.AdClosed += ad_AdClosed;
				ad.AdClicked += ad_AdClicked;// never fires (this API has been around since WP7, how does this not work...?)

				ad.IsTest = testing;
				ad.LoadAd();// this stops the thread
				ad_AdLoaded(null, null);
			});
		}

		public async void Show()
		{
			await WinRTPlugin.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate()
			{
				ad.ShowAd();// this stops the thread
				ad_ShowingOverlay();
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
#elif UNITY_WINRT && !UNITY_WP8
namespace Reign.Plugin
{
	public class AdDuplex_InterstitialAdPlugin_WinRT : IInterstitialAdPlugin
	{
		public IInterstitialAdPlugin Native;

		public delegate void InitNativeMethod(AdDuplex_InterstitialAdPlugin_WinRT plugin, InterstitialAdDesc desc, InterstitialAdCreatedCallbackMethod createdCallback);
		public static InitNativeMethod InitNative;

		public AdDuplex_InterstitialAdPlugin_WinRT(InterstitialAdDesc desc, InterstitialAdCreatedCallbackMethod createdCallback)
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