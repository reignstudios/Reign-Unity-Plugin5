#if REIGN_POSTBUILD
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Windows.UI.Core;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#if UNITY_WP_8_1
using AdDuplex.Universal.Controls.WinPhone.XAML;
using AdDuplex.Universal.WinPhone.WinRT.Models;
using AdDuplex.Universal.WinPhone.WinRT.Core;
#else
using AdDuplex.Universal.Controls.Win.XAML;
using AdDuplex.Universal.Win.WinRT.Models;
using AdDuplex.Universal.Win.WinRT.Core;
#endif

namespace Reign.Plugin
{
	public class AdDuplex_AdPlugin_Native : IAdPlugin
    {
		public AdControl adControl;
		private AdEventCallbackMethod eventCallback;

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

		private async void setVisibleAsync(bool value)
		{
			await WinRTPlugin.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate()
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

		public AdDuplex_AdPlugin_Native(AdDesc desc, AdCreatedCallbackMethod createdCallback)
		{
			init(desc, createdCallback);
		}

		private async void init(AdDesc desc, AdCreatedCallbackMethod createdCallback)
		{
			if (WinRTPlugin.AdGrid == null)
			{
				if (createdCallback != null) createdCallback(false);
				return;
			}

			await WinRTPlugin.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate()
			{
				bool pass = true;
				try
				{
					adControl = new AdControl();
					adControl.AdClick += adControl_Clicked;
					adControl.AdLoaded += adControl_AdRefreshed;
					adControl.AdLoadingError += adControl_ErrorOccurred;
					adControl.AdCovered += adControl_AdCovered;
					adControl.NoAd += adControl_NoAd;
					adControl.IsTest = desc.Testing;
					adControl.CollapseOnError = true;

					adControl.SetValue(Windows.UI.Xaml.Controls.Canvas.ZIndexProperty, 98);
			
					#if UNITY_WP_8_1
					adControl.RefreshInterval = desc.WP8_AdDuplex_RefreshRate;
					adControl.AppKey = desc.WP8_AdDuplex_ApplicationKey;
					adControl.AdUnitId = desc.WP8_AdDuplex_UnitID;
					#else
					adControl.RefreshInterval = desc.WinRT_AdDuplex_RefreshRate;
					adControl.AppKey = desc.WinRT_AdDuplex_ApplicationKey;
					adControl.AdUnitId = desc.WinRT_AdDuplex_UnitID;
					switch (desc.WinRT_AdDuplex_AdSize)
					{
						case WinRT_AdDuplex_AdSize.Tall_160x600:
							adControl.Size = "160x600";
							break;

						case WinRT_AdDuplex_AdSize.Wide_250x125:
							adControl.Size = "250x125";
							break;

						case WinRT_AdDuplex_AdSize.Wide_292x60:
							adControl.Size = "292x60";
							break;

						case WinRT_AdDuplex_AdSize.Wide_300x250:
							adControl.Size = "300x250";
							break;

						case WinRT_AdDuplex_AdSize.Wide_500x130:
							adControl.Size = "500x130";
							break;

						case WinRT_AdDuplex_AdSize.Wide_728x90:
							adControl.Size = "728x90";
							break;

						case WinRT_AdDuplex_AdSize.Square_250x250:
							adControl.Size = "250x250";
							break;

						default:
							Debug.LogError("AdPlugin: Unsuported Ad size");
							break;
					}
					#endif

					#if UNITY_WP_8_1
					setGravity(desc.WP8_AdDuplex_AdGravity);
					#else
					setGravity(desc.WinRT_AdDuplex_AdGravity);
					#endif
					
					eventCallback = desc.EventCallback;
					WinRTPlugin.AdGrid.Children.Add(adControl);
					setVisible(desc.Visible);
					Debug.Log("Created Ad of AppKey: " + adControl.AppKey + " AdUnitID" + adControl.AdUnitId);
				}
				catch (Exception e)
				{
					adControl = null;
					Debug.LogError(e.Message);
				}

				UnityEngine.WSA.Application.InvokeOnAppThread(()=>
				{
					if (createdCallback != null) createdCallback(pass);
				}, false);
			});
		}

		void adControl_Clicked(object sender, AdClickEventArgs e)
		{
			UnityEngine.WSA.Application.InvokeOnAppThread(()=>
			{
				if (eventCallback != null) eventCallback(AdEvents.Clicked, null);
			}, false);
		}

		void adControl_AdRefreshed(object sender, AdLoadedEventArgs e)
		{
			UnityEngine.WSA.Application.InvokeOnAppThread(()=>
			{
				if (eventCallback != null) eventCallback(AdEvents.Refreshed, null);
			}, false);
		}

		void adControl_ErrorOccurred(object sender, AdLoadingErrorEventArgs e)
		{
			UnityEngine.WSA.Application.InvokeOnAppThread(()=>
			{
				Debug.LogError(e.Error.Message);
				if (eventCallback != null) eventCallback(AdEvents.Error, e.Error.Message);
			}, false);
		}

		private void adControl_NoAd(object sender, NoAdEventArgs e)
		{
			UnityEngine.WSA.Application.InvokeOnAppThread(()=>
			{
				Debug.LogError(e.Message);
				if (eventCallback != null) eventCallback(AdEvents.Error, e.Message);
			}, false);
		}

		private void adControl_AdCovered(object sender, AdCoveredEventArgs e)
		{
			UnityEngine.WSA.Application.InvokeOnAppThread(()=>
			{
				string error = "Overlapping XAML object: " + e.CulpritElement.Name;
				Debug.LogError(error);
				if (eventCallback != null) eventCallback(AdEvents.Error, error);
			}, false);
		}

		public void Dispose()
		{
			if (adControl == null) return;
			disposeAsync();
		}

		private async void disposeAsync()
		{
			await WinRTPlugin.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate()
			{
				if (adControl != null)
				{
					adControl.IsEnabled = false;
					WinRTPlugin.AdGrid.Children.Remove(adControl);
					adControl = null;
				}
			});
		}

		public void Refresh()
		{
			Debug.LogError("AdDuplex: Refresh not supported on this platform.");
		}

		public async void SetGravity(AdGravity gravity)
		{
			if (adControl == null) return;

			await WinRTPlugin.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate()
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
#elif UNITY_WINRT && !UNITY_WP8
namespace Reign.Plugin
{
	public class AdDuplex_AdPlugin_WinRT : IAdPlugin
	{
		public IAdPlugin Native;

		public delegate void InitNativeMethod(AdDuplex_AdPlugin_WinRT plugin, AdDesc desc, AdCreatedCallbackMethod createdCallback);
		public static InitNativeMethod InitNative;

		public AdDuplex_AdPlugin_WinRT(AdDesc desc, AdCreatedCallbackMethod createdCallback)
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