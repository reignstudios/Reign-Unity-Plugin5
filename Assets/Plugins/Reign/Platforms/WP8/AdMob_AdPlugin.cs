#if REIGN_POSTBUILD
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GoogleAds;
using System.Windows;
using UnityEngine;

namespace Reign.Plugin
{
	public class AdMob_AdPlugin_Native : IAdPlugin
	{
		private AdView adView;
		private AdEventCallbackMethod eventCallback;
		private bool testing;

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

		private void setVisibleAsync(bool value)
		{
			WinRTPlugin.Dispatcher.BeginInvoke(delegate()
			{
				setVisible(value);
			});
		}

		private void setVisible(bool value)
		{
			visible = value;
			adView.IsEnabled = value;
			adView.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
		}

		public AdMob_AdPlugin_Native(AdDesc desc, AdCreatedCallbackMethod createdCallback)
		{
			eventCallback = desc.EventCallback;
			testing = desc.Testing;

			WinRTPlugin.Dispatcher.BeginInvoke(delegate()
			{
				bool pass = true;
				try
				{
					adView = new AdView();
					setGravity(desc.WP8_AdMob_AdGravity);

					adView.AdUnitID = desc.WP8_AdMob_UnitID;
					adView.Format = desc.WP8_AdMob_AdSize == WP8_AdMob_AdSize.Banner ? AdFormats.Banner : AdFormats.SmartBanner;

					setVisible(desc.Visible);
					adView.ReceivedAd += adView_ReceivedAd;
					adView.FailedToReceiveAd += adView_FailedToReceiveAd;

					WinRTPlugin.AdGrid.Children.Add(adView);

					AdRequest request = new AdRequest();
					//request.ForceTesting = desc.Testing;// Looks like there is a bug in AdMob if this is enabled.
					adView.LoadAd(request);

					Debug.Log("Created Ad of AdUnitID: " + adView.AdUnitID);
				}
				catch (Exception e)
				{
					pass = false;
					Debug.LogError(e.Message);
				}

				if (createdCallback != null) createdCallback(pass);
			});
		}

		void adView_FailedToReceiveAd(object sender, AdErrorEventArgs e)
		{
			ReignServices.InvokeOnUnityThread(delegate
			{
				if (eventCallback != null) eventCallback(AdEvents.Error, e.ErrorCode.ToString());
			});
		}

		void adView_ReceivedAd(object sender, AdEventArgs e)
		{
			ReignServices.InvokeOnUnityThread(delegate
			{
				if (eventCallback != null) eventCallback(AdEvents.Refreshed, null);
			});
		}

		public void Dispose()
		{
			if (adView == null) return;
			disposeAsync();
		}

		private void disposeAsync()
		{
			WinRTPlugin.Dispatcher.BeginInvoke(delegate()
			{
				if (adView != null)
				{
					adView.IsEnabled = false;
					WinRTPlugin.AdGrid.Children.Remove(adView);
					adView = null;
				}
			});
		}

		public void Refresh()
		{
			WinRTPlugin.Dispatcher.BeginInvoke(delegate()
			{
				AdRequest request = new AdRequest();
				request.ForceTesting = testing;
				adView.LoadAd(request);
			});
		}

		public void SetGravity(AdGravity gravity)
		{
			if (adView == null) return;

			WinRTPlugin.Dispatcher.BeginInvoke(delegate()
			{
				setGravity(gravity);
			});
		}

		private void setGravity(AdGravity gravity)
		{
			switch (gravity)
			{
				case AdGravity.BottomLeft:
					adView.HorizontalAlignment = HorizontalAlignment.Left;
					adView.VerticalAlignment = VerticalAlignment.Bottom;
					break;

				case AdGravity.BottomRight:
					adView.HorizontalAlignment = HorizontalAlignment.Right;
					adView.VerticalAlignment = VerticalAlignment.Bottom;
					break;

				case AdGravity.BottomCenter:
					adView.HorizontalAlignment = HorizontalAlignment.Center;
					adView.VerticalAlignment = VerticalAlignment.Bottom;
					break;

				case AdGravity.TopLeft:
					adView.HorizontalAlignment = HorizontalAlignment.Left;
					adView.VerticalAlignment = VerticalAlignment.Top;
					break;

				case AdGravity.TopRight:
					adView.HorizontalAlignment = HorizontalAlignment.Right;
					adView.VerticalAlignment = VerticalAlignment.Top;
					break;

				case AdGravity.TopCenter:
					adView.HorizontalAlignment = HorizontalAlignment.Center;
					adView.VerticalAlignment = VerticalAlignment.Top;
					break;

				case AdGravity.CenterScreen:
					adView.HorizontalAlignment = HorizontalAlignment.Center;
					adView.VerticalAlignment = VerticalAlignment.Center;
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
#elif UNITY_WP8
namespace Reign.Plugin
{
	public class AdMob_AdPlugin_WP8 : IAdPlugin
	{
		public IAdPlugin Native;

		public delegate void InitNativeMethod(AdMob_AdPlugin_WP8 plugin, AdDesc desc, AdCreatedCallbackMethod createdCallback);
		public static InitNativeMethod InitNative;

		public AdMob_AdPlugin_WP8(AdDesc desc, AdCreatedCallbackMethod createdCallback)
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