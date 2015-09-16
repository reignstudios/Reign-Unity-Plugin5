#if UNITY_IPHONE
using System;
using System.Runtime.InteropServices;

namespace Reign.Plugin
{
    public class iAd_AdPlugin_iOS : IAdPlugin
    {
    	private bool visible;
		public bool Visible
		{
			get {return visible;}
			set
			{
				visible = value;
				#if !IOS_DISABLE_APPLE_ADS
				iAd_SetAdVisible(native, value);
				#endif
			}
		}
		
		private IntPtr native;
		private AdEventCallbackMethod eventCallback;

		#if !IOS_DISABLE_APPLE_ADS
		[DllImport("__Internal", EntryPoint="iAd_InitAd")]
		private static extern IntPtr iAd_InitAd(bool testing);
		
		[DllImport("__Internal", EntryPoint="iAd_DisposeAd")]
		private static extern void iAd_DisposeAd(IntPtr native);
		
		[DllImport("__Internal", EntryPoint="iAd_CreateAd")]
		private static extern void iAd_CreateAd(IntPtr native, int gravity);
		
		[DllImport("__Internal", EntryPoint="iAd_SetAdGravity")]
		private static extern void iAd_SetAdGravity(IntPtr native, int gravity);
		
		[DllImport("__Internal", EntryPoint="iAd_SetAdVisible")]
		private static extern void iAd_SetAdVisible(IntPtr native, bool visible);
		
		[DllImport("__Internal", EntryPoint="iAd_AdHasEvents")]
		private static extern bool iAd_AdHasEvents(IntPtr native);
		
		[DllImport("__Internal", EntryPoint="iAd_GetNextAdEvent")]
		private static extern IntPtr iAd_GetNextAdEvent(IntPtr native);
		#endif

		public iAd_AdPlugin_iOS(AdDesc desc, AdCreatedCallbackMethod createdCallback)
		{
			#if !IOS_DISABLE_APPLE_ADS
			eventCallback = desc.EventCallback;
			native = iAd_InitAd(desc.Testing);
			int gravity = convertGravity(desc.iOS_iAd_AdGravity);
			
			iAd_CreateAd(native, gravity);
			Visible = desc.Visible;
			if (createdCallback != null) createdCallback(true);
			#endif
		}
		
		~iAd_AdPlugin_iOS()
		{
			Dispose();
		}

		public void Dispose()
		{
			#if !IOS_DISABLE_APPLE_ADS
			iAd_DisposeAd(native);
			native = IntPtr.Zero;
			#endif
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
			#if !IOS_DISABLE_APPLE_ADS
			int gravityIndex = convertGravity(gravity);
			iAd_SetAdGravity(native, gravityIndex);
			#endif
		}
		
		public void Refresh()
		{
			// do nothing...
		}
		
		public void Update()
		{
			#if !IOS_DISABLE_APPLE_ADS
			if (eventCallback != null && iAd_AdHasEvents(native))
			{
				IntPtr ptr = iAd_GetNextAdEvent(native);
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