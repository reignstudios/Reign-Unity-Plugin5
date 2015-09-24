#if REIGN_POSTBUILD
using System;

#if WINDOWS_PHONE
using Microsoft.Phone.Tasks;
#endif

namespace Reign.Plugin
{
    public class MarketingPlugin_Native : IIMarketingPlugin
    {
		#if UNITY_METRO
		#if UNITY_WP_8_1
		private void async_OpenStore(string url)
		{
			UnityEngine.Application.OpenURL("ms-windows-store:navigate?appid=" + url);
		}
		#else
		private async void async_OpenStore(string url)
		{
			var uri = new Uri("ms-windows-store:PDP?PFN=" + url);
			await Windows.System.Launcher.LaunchUriAsync(uri);
		}
		#endif
		#endif

		public void OpenStore(MarketingDesc desc)
		{
			#if WINDOWS_PHONE
			var task = new MarketplaceDetailTask();
			task.Show();
			#elif UNITY_WP_8_1
			async_OpenStore(desc.WP8_AppID);
			#else
			async_OpenStore(desc.Win8_PackageFamilyName);
			#endif
		}

		#if UNITY_METRO
		#if UNITY_WP_8_1
		private void async_OpenStoreForReview(string url)
		{
			UnityEngine.Application.OpenURL("ms-windows-store:reviewapp?appid=" + url);
		}
		#else
		private async void async_OpenStoreForReview(string url)
		{
			var uri = new Uri("ms-windows-store:Review?PFN=" + url);
			await Windows.System.Launcher.LaunchUriAsync(uri);
		}
		#endif
		#endif

		public void OpenStoreForReview(MarketingDesc desc)
		{
			#if WINDOWS_PHONE
			var task = new MarketplaceReviewTask();
			task.Show();
			#elif UNITY_WP_8_1
			async_OpenStoreForReview(desc.WP8_AppID);
			#else
			async_OpenStoreForReview(desc.Win8_PackageFamilyName);
			#endif
		}
    }
}
#elif UNITY_WINRT
namespace Reign.Plugin
{
    public class MarketingPlugin_WinRT : IIMarketingPlugin
    {
		public IIMarketingPlugin Native;

		public delegate void InitNativeMethod(MarketingPlugin_WinRT plugin);
		public static InitNativeMethod InitNative;

		public MarketingPlugin_WinRT()
		{
			InitNative(this);
		}

		public void OpenStore(MarketingDesc desc)
		{
			Native.OpenStore(desc);
		}

		public void OpenStoreForReview(MarketingDesc desc)
		{
			Native.OpenStoreForReview(desc);
		}
    }
}
#endif