#if REIGN_POSTBUILD
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Windows.UI.Core;

#if UNITY_METRO
#if UNITY_WP_8_1
using Microsoft.Advertising.Mobile.UI;
using Microsoft.Advertising.Mobile.Common;
#else
using Microsoft.Advertising.WinRT.UI;
#endif
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#endif

#if WINDOWS_PHONE
using Microsoft.Advertising.Mobile.UI;
using Microsoft.Advertising;
using System.Windows;
using System.Windows.Threading;
#endif

namespace Reign.Plugin
{
	public class MicrosoftAdvertising_AdPlugin_Native : IAdPlugin
    {
		public AdControl adControl;
		private AdEventCallbackMethod eventCallback;
		private DispatcherTimer manualRefreshTimer;

		private bool visible;
		public bool Visible
		{
			get {return visible;}
			set
			{
				visible = value;
				setVisibleAsync(value);
			}
		}

		#if WINDOWS_PHONE
		private void setVisibleAsync(bool value)
		#else
		private async void setVisibleAsync(bool value)
		#endif
		{
			#if WINDOWS_PHONE
			WinRTPlugin.Dispatcher.BeginInvoke(delegate()
			#else
			await WinRTPlugin.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate()
			#endif
			{
				setVisible(value);
			});
		}

		private void setVisible(bool value)
		{
			visible = value;
			adControl.IsEnabled = value;
			adControl.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
		}

		public MicrosoftAdvertising_AdPlugin_Native(AdDesc desc, AdCreatedCallbackMethod createdCallback)
		{
			init(desc, createdCallback);
		}

		#if WINDOWS_PHONE
		private void init(AdDesc desc, AdCreatedCallbackMethod createdCallback)
		#else
		private async void init(AdDesc desc, AdCreatedCallbackMethod createdCallback)
		#endif
		{
			if (WinRTPlugin.AdGrid == null)
			{
				if (createdCallback != null) createdCallback(false);
				return;
			}

			#if WINDOWS_PHONE
			WinRTPlugin.Dispatcher.BeginInvoke(delegate()
			#else
			await WinRTPlugin.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate()
			#endif
			{
				bool pass = true;
				try
				{
					adControl = new AdControl();
					#if WINDOWS_PHONE || UNITY_WP_8_1
					adControl.IsAutoRefreshEnabled = desc.WP8_MicrosoftAdvertising_UseBuiltInRefresh;
					if (!desc.WP8_MicrosoftAdvertising_UseBuiltInRefresh)
					{
						manualRefreshTimer = new DispatcherTimer();
						manualRefreshTimer.Interval = TimeSpan.FromSeconds(desc.WP8_MicrosoftAdvertising_RefreshRate);
						manualRefreshTimer.Tick += timer_Tick;
						manualRefreshTimer.Start();
					}

					adControl.IsEngagedChanged += adControl_IsEngagedChanged;
					adControl.AdRefreshed += adControl_AdRefreshed;
					#else
					adControl.IsAutoRefreshEnabled = desc.WinRT_MicrosoftAdvertising_UseBuiltInRefresh;
					if (!desc.WinRT_MicrosoftAdvertising_UseBuiltInRefresh)
					{
						manualRefreshTimer = new DispatcherTimer();
						manualRefreshTimer.Interval = TimeSpan.FromSeconds(desc.WinRT_MicrosoftAdvertising_RefreshRate);
						manualRefreshTimer.Tick += timer_Tick;
						manualRefreshTimer.Start();
					}

					adControl.IsEngagedChanged += adControl_IsEngagedChanged;
					adControl.AdRefreshed += adControl_AdRefreshed;
					#endif

					adControl.ErrorOccurred += adControl_ErrorOccurred;
					#if WINDOWS_PHONE
					adControl.SetValue(System.Windows.Controls.Canvas.ZIndexProperty, 98);
					#else
					adControl.SetValue(Windows.UI.Xaml.Controls.Canvas.ZIndexProperty, 98);
					#endif
			
					#if WINDOWS_PHONE || UNITY_WP_8_1
					adControl.ApplicationId = desc.Testing ? "test_client" : desc.WP8_MicrosoftAdvertising_ApplicationID;
					adControl.AdUnitId = desc.WP8_MicrosoftAdvertising_UnitID;
					switch (desc.WP8_MicrosoftAdvertising_AdSize)
					{
						case WP8_MicrosoftAdvertising_AdSize.Wide_640x100:
							adControl.Width = 640;
							adControl.Height = 100;
							if (desc.Testing) adControl.AdUnitId = "Image640_100";
							break;

						case WP8_MicrosoftAdvertising_AdSize.Wide_480x80:
							adControl.Width = 480;
							adControl.Height = 80;
							if (desc.Testing) adControl.AdUnitId = "Image480_80";
							break;

						case WP8_MicrosoftAdvertising_AdSize.Wide_320x50:
							adControl.Width = 320;
							adControl.Height = 50;
							if (desc.Testing) adControl.AdUnitId = "Image320_50";
							break;

						case WP8_MicrosoftAdvertising_AdSize.Wide_300x50:
							adControl.Width = 300;
							adControl.Height = 50;
							if (desc.Testing) adControl.AdUnitId = "Image300_50";
							break;

						default:
							Debug.LogError("AdPlugin: Unsuported Ad size");
							break;
					}
					#elif UNITY_METRO
					adControl.ApplicationId = desc.Testing ? "d25517cb-12d4-4699-8bdc-52040c712cab" : desc.WinRT_MicrosoftAdvertising_ApplicationID;
					adControl.AdUnitId = desc.WinRT_MicrosoftAdvertising_UnitID;
					switch (desc.WinRT_MicrosoftAdvertising_AdSize)
					{
						case WinRT_MicrosoftAdvertising_AdSize.Tall_160x600:
							adControl.Width = 160;
							adControl.Height = 600;
							if (desc.Testing) adControl.AdUnitId = "10043134";
							break;

						case WinRT_MicrosoftAdvertising_AdSize.Tall_300x600:
							adControl.Width = 300;
							adControl.Height = 600;
							if (desc.Testing) adControl.AdUnitId = "10043030";
							break;

						case WinRT_MicrosoftAdvertising_AdSize.Wide_300x250:
							adControl.Width = 300;
							adControl.Height = 250;
							if (desc.Testing) adControl.AdUnitId = "10043008";
							break;

						case WinRT_MicrosoftAdvertising_AdSize.Wide_728x90:
							adControl.Width = 728;
							adControl.Height = 90;
							if (desc.Testing) adControl.AdUnitId = "10042998";
							break;

						case WinRT_MicrosoftAdvertising_AdSize.Square_250x250:
							adControl.Width = 250;
							adControl.Height = 250;
							if (desc.Testing) adControl.AdUnitId = "10043105";
							break;

						default:
							Debug.LogError("AdPlugin: Unsuported Ad size");
							break;
					}
					#endif

					#if WINDOWS_PHONE || UNITY_WP_8_1
					setGravity(desc.WP8_MicrosoftAdvertising_AdGravity);
					#else
					setGravity(desc.WinRT_MicrosoftAdvertising_AdGravity);
					#endif
					
					eventCallback = desc.EventCallback;
					WinRTPlugin.AdGrid.Children.Add(adControl);
					setVisible(desc.Visible);
					Debug.Log("Created Ad of ApplicationID: " + adControl.ApplicationId + " AdUnitID" + adControl.AdUnitId);
				}
				catch (Exception e)
				{
					adControl = null;
					Debug.LogError(e.Message);
				}

				if (createdCallback != null) createdCallback(pass);
			});
		}

		#if WINDOWS_PHONE
		void timer_Tick(object sender, EventArgs e)
		{
			Refresh();
		}
		#else
		void timer_Tick(object sender, object e)
		{
			Refresh();
		}
		#endif

		#if WINDOWS_PHONE
		void adControl_IsEngagedChanged(object sender, EventArgs e)
		{
			ReignServices.InvokeOnUnityThread(delegate
			{
				if (adControl.IsEngaged && eventCallback != null) eventCallback(AdEvents.Clicked, null);
			});
		}

		void adControl_AdRefreshed(object sender, EventArgs e)
		{
			ReignServices.InvokeOnUnityThread(delegate
			{
				if (eventCallback != null) eventCallback(AdEvents.Refreshed, null);
			});
		}
		#else
		void adControl_IsEngagedChanged(object sender, RoutedEventArgs e)
		{
			ReignServices.InvokeOnUnityThread(delegate
			{
				if (adControl.IsEngaged && eventCallback != null) eventCallback(AdEvents.Clicked, null);
			});
		}

		void adControl_AdRefreshed(object sender, RoutedEventArgs e)
		{
			ReignServices.InvokeOnUnityThread(delegate
			{
				if (eventCallback != null) eventCallback(AdEvents.Refreshed, null);
			});
		}
		#endif

		void adControl_ErrorOccurred(object sender, AdErrorEventArgs e)
		{
			ReignServices.InvokeOnUnityThread(delegate
			{
				Debug.LogError(e.Error.Message);
				if (eventCallback != null) eventCallback(AdEvents.Error, e.Error.Message);
			});
		}

		public void Dispose()
		{
			if (adControl == null) return;
			disposeAsync();
		}

		#if WINDOWS_PHONE
		private void disposeAsync()
		#else
		private async void disposeAsync()
		#endif
		{
			#if WINDOWS_PHONE
			WinRTPlugin.Dispatcher.BeginInvoke(delegate()
			#else
			await WinRTPlugin.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate()
			#endif
			{
				if (adControl != null)
				{
					adControl.IsEnabled = false;
					WinRTPlugin.AdGrid.Children.Remove(adControl);
					adControl = null;
				}

				if (manualRefreshTimer != null)
				{
					manualRefreshTimer.Stop();
					manualRefreshTimer.Tick -= timer_Tick;
					manualRefreshTimer = null;
				}
			});
		}

		#if WINDOWS_PHONE
		public void Refresh()
		#else
		public async void Refresh()
		#endif
		{
			#if WINDOWS_PHONE
			WinRTPlugin.Dispatcher.BeginInvoke(delegate()
			#else
			await WinRTPlugin.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate()
			#endif
			{
				adControl.Refresh();
			});
		}

		#if WINDOWS_PHONE
		public void SetGravity(AdGravity gravity)
		#else
		public async void SetGravity(AdGravity gravity)
		#endif
		{
			if (adControl == null) return;

			#if WINDOWS_PHONE
			WinRTPlugin.Dispatcher.BeginInvoke(delegate()
			#else
			await WinRTPlugin.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate()
			#endif
			{
				setGravity(gravity);
			});
		}

		private void setGravity(AdGravity gravity)
		{
			switch (gravity)
			{
				case AdGravity.BottomLeft:
					adControl.HorizontalAlignment = HorizontalAlignment.Left;
					adControl.VerticalAlignment = VerticalAlignment.Bottom;
					break;

				case AdGravity.BottomRight:
					adControl.HorizontalAlignment = HorizontalAlignment.Right;
					adControl.VerticalAlignment = VerticalAlignment.Bottom;
					break;

				case AdGravity.BottomCenter:
					adControl.HorizontalAlignment = HorizontalAlignment.Center;
					adControl.VerticalAlignment = VerticalAlignment.Bottom;
					break;

				case AdGravity.TopLeft:
					adControl.HorizontalAlignment = HorizontalAlignment.Left;
					adControl.VerticalAlignment = VerticalAlignment.Top;
					break;

				case AdGravity.TopRight:
					adControl.HorizontalAlignment = HorizontalAlignment.Right;
					adControl.VerticalAlignment = VerticalAlignment.Top;
					break;

				case AdGravity.TopCenter:
					adControl.HorizontalAlignment = HorizontalAlignment.Center;
					adControl.VerticalAlignment = VerticalAlignment.Top;
					break;

				case AdGravity.CenterScreen:
					adControl.HorizontalAlignment = HorizontalAlignment.Center;
					adControl.VerticalAlignment = VerticalAlignment.Center;
					break;

				default:
					Debug.LogError("AdPlugin: Unsuported Ad gravity");
					break;
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
#elif UNITY_WINRT
namespace Reign.Plugin
{
	public class MicrosoftAdvertising_AdPlugin_WinRT : IAdPlugin
	{
		public IAdPlugin Native;

		public delegate void InitNativeMethod(MicrosoftAdvertising_AdPlugin_WinRT plugin, AdDesc desc, AdCreatedCallbackMethod createdCallback);
		public static InitNativeMethod InitNative;

		public MicrosoftAdvertising_AdPlugin_WinRT(AdDesc desc, AdCreatedCallbackMethod createdCallback)
		{
			InitNative(this, desc, createdCallback);
		}

		public bool Visible
		{
			get {return Native.Visible;}
			set {Native.Visible = value;}
		}

		public void Dispose()
		{
			Native.Dispose();
		}

		public void SetGravity(AdGravity gravity)
		{
			Native.SetGravity(gravity);
		}

		public void Refresh()
		{
			Native.Refresh();
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