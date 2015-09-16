#if UNITY_IPHONE
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Reign.Plugin
{
	public class AdMob_AdPlugin_iOS : IAdPlugin
	{
		private bool visible;
		public bool Visible
		{
			get {return visible;}
			set
			{
				visible = value;
				#if !IOS_DISABLE_GOOGLE_ADS
				AdMob_SetAdVisible(native, value);
				#endif
			}
		}
		
		private IntPtr native;
		private AdEventCallbackMethod eventCallback;

		#if !IOS_DISABLE_GOOGLE_ADS
		[DllImport("__Internal", EntryPoint="AdMob_InitAd")]
		private static extern IntPtr AdMob_InitAd(bool testing);
		
		[DllImport("__Internal", EntryPoint="AdMob_DisposeAd")]
		private static extern void AdMob_DisposeAd(IntPtr native);
		
		[DllImport("__Internal", EntryPoint="AdMob_CreateAd")]
		private static extern void AdMob_CreateAd(IntPtr native, int gravity, int adSizeIndex, string unitID);
		
		[DllImport("__Internal", EntryPoint="AdMob_SetAdGravity")]
		private static extern void AdMob_SetAdGravity(IntPtr native, int gravity);
		
		[DllImport("__Internal", EntryPoint="AdMob_SetAdVisible")]
		private static extern void AdMob_SetAdVisible(IntPtr native, bool visible);
		[DllImport("__Internal", EntryPoint="AdMob_Refresh")]
		private static extern void AdMob_Refresh(IntPtr native);
		
		[DllImport("__Internal", EntryPoint="AdMob_AdHasEvents")]
		private static extern bool AdMob_AdHasEvents(IntPtr native);
		
		[DllImport("__Internal", EntryPoint="AdMob_GetNextAdEvent")]
		private static extern IntPtr AdMob_GetNextAdEvent(IntPtr native);
		#endif

		public AdMob_AdPlugin_iOS(AdDesc desc, AdCreatedCallbackMethod createdCallback)
		{
			#if !IOS_DISABLE_GOOGLE_ADS
			bool pass = true;
			try
			{
				eventCallback = desc.EventCallback;
				native = AdMob_InitAd(desc.Testing);
				int gravity = convertGravity(desc.iOS_AdMob_AdGravity);
				
				AdMob_CreateAd(native, gravity, convertAdSize(desc.iOS_AdMob_AdSize), desc.iOS_AdMob_UnitID);
				Visible = desc.Visible;
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message);
				pass = false;
			}
				
			if (createdCallback != null) createdCallback(pass);
			#endif
		}
		
		~AdMob_AdPlugin_iOS()
		{
			Dispose();
		}

		public void Dispose()
		{
			#if !IOS_DISABLE_GOOGLE_ADS
			AdMob_DisposeAd(native);
			native = IntPtr.Zero;
			#endif
		}
		
		private int convertAdSize(iOS_AdMob_AdSize adSize)
		{
			switch (adSize)
			{
				case iOS_AdMob_AdSize.Banner_320x50: return 0;
				case iOS_AdMob_AdSize.FullBanner_468x60: return 1;
				case iOS_AdMob_AdSize.Leaderboard_728x90: return 2;
				case iOS_AdMob_AdSize.MediumRectangle_300x250: return 3;
				case iOS_AdMob_AdSize.SmartBannerLandscape: return 4;
				case iOS_AdMob_AdSize.SmartBannerPortrait: return 5;
			}
			
			return 0;
		}
		
		private int convertGravity(AdGravity gravity)
		{
			int gravityIndex = 0;
			switch (gravity)
			{
				case AdGravity.BottomLeft: gravityIndex = 0; break;
				case AdGravity.BottomRight: gravityIndex = 1; break;
				case AdGravity.BottomCenter: gravityIndex = 2; break;
				case AdGravity.TopLeft: gravityIndex = 3; break;
				case AdGravity.TopRight: gravityIndex = 4; break;
				case AdGravity.TopCenter: gravityIndex = 5; break;
				case AdGravity.CenterScreen: gravityIndex = 6; break;
			}
			
			return gravityIndex;
		}

		public void SetGravity(AdGravity gravity)
		{
			#if !IOS_DISABLE_GOOGLE_ADS
			int gravityIndex = convertGravity(gravity);
			AdMob_SetAdGravity(native, gravityIndex);
			#endif
		}
		
		public void Refresh()
		{
			#if !IOS_DISABLE_GOOGLE_ADS
			AdMob_Refresh(native);
			#endif
		}
		
		public void Update()
		{
			#if !IOS_DISABLE_GOOGLE_ADS
			if (eventCallback != null && AdMob_AdHasEvents(native))
			{
				IntPtr ptr = AdMob_GetNextAdEvent(native);
				string message = Marshal.PtrToStringAnsi(ptr);
				var values = message.Split(':');
				switch (values[0])
				{
					case "Refreshed": eventCallback(AdEvents.Refreshed, null); break;
					case "Clicked": eventCallback(AdEvents.Clicked, null); break;
					case "Error": eventCallback(AdEvents.Error, values[1]); break;
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