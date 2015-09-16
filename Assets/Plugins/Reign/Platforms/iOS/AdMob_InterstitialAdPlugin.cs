#if UNITY_IPHONE
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Reign.Plugin
{
	public class AdMob_InterstitialAdPlugin_iOS : IInterstitialAdPlugin
	{
		private IntPtr native;
		private InterstitialAdEventCallbackMethod eventCallback;

		#if !IOS_DISABLE_GOOGLE_ADS
		[DllImport("__Internal", EntryPoint="AdMob_Interstitial_InitAd")]
		private static extern IntPtr AdMob_Interstitial_InitAd(bool testing);
		
		[DllImport("__Internal", EntryPoint="AdMob_Interstitial_DisposeAd")]
		private static extern void AdMob_Interstitial_DisposeAd(IntPtr native);
		
		[DllImport("__Internal", EntryPoint="AdMob_Interstitial_CreateAd")]
		private static extern void AdMob_Interstitial_CreateAd(IntPtr native, string unitID);
		
		[DllImport("__Internal", EntryPoint="AdMob_Interstitial_AdHasEvents")]
		private static extern bool AdMob_Interstitial_AdHasEvents(IntPtr native);
		
		[DllImport("__Internal", EntryPoint="AdMob_Interstitial_GetNextAdEvent")]
		private static extern IntPtr AdMob_Interstitial_GetNextAdEvent(IntPtr native);
		
		[DllImport("__Internal", EntryPoint="AdMob_Interstitial_Show")]
		private static extern void AdMob_Interstitial_Show(IntPtr native);
		
		[DllImport("__Internal", EntryPoint="AdMob_Interstitial_Cache")]
		private static extern void AdMob_Interstitial_Cache(IntPtr native);
		#endif
		
		public AdMob_InterstitialAdPlugin_iOS (InterstitialAdDesc desc, InterstitialAdCreatedCallbackMethod callback)
		{
			#if !IOS_DISABLE_GOOGLE_ADS
			bool pass = true;
			try
			{
				eventCallback = desc.EventCallback;
				native = AdMob_Interstitial_InitAd(desc.Testing);
				
				AdMob_Interstitial_CreateAd(native, desc.iOS_AdMob_UnitID);
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message);
				pass = false;
			}
			
			if (callback != null) callback(pass);
			#endif
		}
		
		~AdMob_InterstitialAdPlugin_iOS()
		{
			Dispose();
		}

		public void Dispose()
		{
			#if !IOS_DISABLE_GOOGLE_ADS
			AdMob_Interstitial_DisposeAd(native);
			native = IntPtr.Zero;
			#endif
		}
		
		public void Cache()
		{
			#if !IOS_DISABLE_GOOGLE_ADS
			AdMob_Interstitial_Cache(native);
			#endif
		}
		
		public void Show()
		{
			#if !IOS_DISABLE_GOOGLE_ADS
			AdMob_Interstitial_Show(native);
			#endif
		}
		
		public void Update()
		{
			#if !IOS_DISABLE_GOOGLE_ADS
			if (eventCallback != null && AdMob_Interstitial_AdHasEvents(native))
			{
				IntPtr ptr = AdMob_Interstitial_GetNextAdEvent(native);
				string message = Marshal.PtrToStringAnsi(ptr);
				var values = message.Split(':');
				switch (values[0])
				{
					case "Cached": eventCallback(InterstitialAdEvents.Cached, null); break;
					case "Shown": eventCallback(InterstitialAdEvents.Shown, null); break;
					case "Canceled": eventCallback(InterstitialAdEvents.Canceled, null); break;
					case "Clicked": eventCallback(InterstitialAdEvents.Clicked, null); break;
					case "Error": eventCallback(InterstitialAdEvents.Error, values[1]); break;
				}
			}
			#endif
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
#endif