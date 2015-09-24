#if REIGN_POSTBUILD
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Reign.Plugin;
using System.Threading;

#if UNITY_METRO
#if !UNITY_WP_8_1
using Windows.UI.ApplicationSettings;
#endif
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.ViewManagement;
using Windows.ApplicationModel.Activation;
#endif

#if WINDOWS_PHONE
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows;
#endif

namespace Reign
{
	public static class WinRTPlugin
	{
		public static Grid AdGrid {get; private set;}

		#if UNITY_METRO
		public static CoreWindow CoreWindow {get; private set;}
		#if !UNITY_WP_8_1
		private static string privacyPolicyURL;
		#endif

		#if UNITY_WP_8_1
		public static void Init(Grid adGrid)
		#else
		public static void Init(Grid adGrid, string privacyPolicyURL, bool usePrivacyPolicy)
		#endif
		{
			CoreWindow = CoreWindow.GetForCurrentThread();
			WinRTPlugin.AdGrid = adGrid;

			#if !UNITY_WP_8_1
			if (usePrivacyPolicy)
			{
				WinRTPlugin.privacyPolicyURL = privacyPolicyURL;
				SettingsPane.GetForCurrentView().CommandsRequested += showPrivacyPolicy;
			}
			#endif

			initMethodPointers();
		}

		#if UNITY_WP_8_1
		public static void OnActivated(IActivatedEventArgs args)
		{
			switch (args.Kind)
			{
				case ActivationKind.PickFileContinuation:
					var data = args as IFileOpenPickerContinuationEventArgs;
					StreamPlugin_Native.loadFileDialog_Callback(data);
					break;
			}
		}
		#endif

		#if !UNITY_WP_8_1
		private static void showPrivacyPolicy(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
		{
			var privacyPolicyCommand = new SettingsCommand("privacyPolicy","Privacy Policy", (uiCommand) => {launchPrivacyPolicyUrl();});
			args.Request.ApplicationCommands.Add(privacyPolicyCommand);
		}

		private static async void launchPrivacyPolicyUrl()
		{
			Uri privacyPolicyUrl = new Uri(privacyPolicyURL);
			var result = await Windows.System.Launcher.LaunchUriAsync(privacyPolicyUrl);
		}
		#endif
		#endif

		#if WINDOWS_PHONE
		public static Dispatcher Dispatcher;

		/// <summary>
		/// Used to reference objects required by the Reign plugin.
		/// </summary>
		/// <param name="adGrid">Pass in the "DrawingSurfaceBackground" object</param>
		/// <param name="dispatcher">Pass in the "Dispatcher" object</param>
		public static void Init(Grid adGrid, Dispatcher dispatcher)
		{
			WinRTPlugin.Dispatcher = dispatcher;
			WinRTPlugin.AdGrid = adGrid;
			initMethodPointers();
		}

		public static void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
		{
			if (e.ExceptionObject.StackTrace.IndexOf("Google") >= 0)
			{
				e.Handled = true; // Ignore google admob control exceptions
			}
		}
		#endif

		private static void initMethodPointers()
		{
			StreamPlugin_WinRT.InitNative = init_StreamPlugin;
			SocialPlugin_WinRT.InitNative = init_SocialPlugin;
			MicrosoftStore_InAppPurchasePlugin_WinRT.InitNative = init_MicrosoftStore_InAppPurchasePlugin;
			MicrosoftAdvertising_AdPlugin_WinRT.InitNative = init_MicrosoftAdvertising_AdPlugin;
			MessageBoxPlugin_WinRT.InitNative = init_MessageBoxPlugin;
			MarketingPlugin_WinRT.InitNative = init_MarketingPlugin;
			EmailPlugin_WinRT.InitNative = init_EmailPlugin;
			
			#if UNITY_METRO
			AdDuplex_AdPlugin_WinRT.InitNative = init_AdDuplex_AdPlugin;
			AdDuplex_InterstitialAdPlugin_WinRT.InitNative = init_AdDuplex_InterstitialAdPlugin;
			#endif

			#if WINDOWS_PHONE
			AdMob_AdPlugin_WP8.InitNative = init_AdMob_AdPlugin;
			AdMob_InterstitialAdPlugin_WP8.InitNative = init_AdMob_InterstitialAdPlugin;
			#endif

			// classic types
			System.Text.Reign.Encoding.GetEncoding = Encoding_GetEncoding_get;
			System.Text.Reign.Encoding.Singleton.GetString = WinRTLegacy.Text.Encoding.Default.GetString;
			System.Text.Reign.Encoding.Singleton.GetBytes = WinRTLegacy.Text.Encoding.Default.GetBytes;

			System.Text.Reign.Encoding.ASCII.GetString = WinRTLegacy.Text.Encoding.ASCII.GetString;
			System.Text.Reign.Encoding.ASCII.GetBytes = WinRTLegacy.Text.Encoding.ASCII.GetBytes;

			System.Text.Reign.Encoding.UTF8.GetString = WinRTLegacy.Text.Encoding.UTF8.GetString;
			System.Text.Reign.Encoding.UTF8.GetBytes = WinRTLegacy.Text.Encoding.UTF8.GetBytes;

			System.Text.Reign.CultureInfo.ANSICodePage = CultureInfo_ANSICodePage_Get;
		}

		private static System.Text.Reign.Encoding Encoding_GetEncoding_get(int codepage)
		{
			return System.Text.Reign.Encoding.Singleton;
		}

		private static int CultureInfo_ANSICodePage_Get()
		{
			return WinRTLegacy.Text.Encoding.Default.CodePage;
		}

		private static void init_StreamPlugin(StreamPlugin_WinRT plugin)
		{
			plugin.Native = new StreamPlugin_Native();
		}

		private static void init_SocialPlugin(SocialPlugin_WinRT plugin)
		{
			plugin.Native = new SocialPlugin_Native();
		}

		private static void init_MicrosoftStore_InAppPurchasePlugin(MicrosoftStore_InAppPurchasePlugin_WinRT plugin, InAppPurchaseDesc desc, InAppPurchaseCreatedCallbackMethod createdCallback)
		{
			#if !WINRT_DISABLE_MS_IAP
			plugin.Native = new MicrosoftStore_InAppPurchasePlugin_Native(desc, createdCallback);
			#endif
		}

		private static void init_MicrosoftAdvertising_AdPlugin(MicrosoftAdvertising_AdPlugin_WinRT plugin, AdDesc desc, AdCreatedCallbackMethod createdCallback)
		{
			#if !WINRT_DISABLE_MS_ADS
			plugin.Native = new MicrosoftAdvertising_AdPlugin_Native(desc, createdCallback);
			#endif
		}

		#if UNITY_METRO
		private static void init_AdDuplex_AdPlugin(AdDuplex_AdPlugin_WinRT plugin, AdDesc desc, AdCreatedCallbackMethod createdCallback)
		{
			#if !WINRT_DISABLE_ADDUPLEX_ADS
			plugin.Native = new AdDuplex_AdPlugin_Native(desc, createdCallback);
			#endif
		}

		private static void init_AdDuplex_InterstitialAdPlugin(AdDuplex_InterstitialAdPlugin_WinRT plugin, InterstitialAdDesc desc, InterstitialAdCreatedCallbackMethod createdCallback)
		{
			#if UNITY_WP_8_1 && !WINRT_DISABLE_ADDUPLEX_ADS
			plugin.Native = new AdDuplex_InterstitialAdPlugin_Native(desc, createdCallback);
			#else
			plugin.Native = new Dumy_InterstitialAdPlugin(desc, createdCallback);
			#endif
		}
		#endif

		private static void init_MessageBoxPlugin(MessageBoxPlugin_WinRT plugin)
		{
			plugin.Native = new MessageBoxPlugin_Native();
		}

		private static void init_MarketingPlugin(MarketingPlugin_WinRT plugin)
		{
			plugin.Native = new MarketingPlugin_Native();
		}

		private static void init_EmailPlugin(EmailPlugin_WinRT plugin)
		{
			plugin.Native = new EmailPlugin_Native();
		}

		#if WINDOWS_PHONE
		private static void init_AdMob_AdPlugin(AdMob_AdPlugin_WP8 plugin, AdDesc desc, AdCreatedCallbackMethod createdCallback)
		{
			#if !WINRT_DISABLE_GOOGLE_ADS
			plugin.Native = new AdMob_AdPlugin_Native(desc, createdCallback);
			#endif
		}

		private static void init_AdMob_InterstitialAdPlugin(AdMob_InterstitialAdPlugin_WP8 plugin, InterstitialAdDesc desc, InterstitialAdCreatedCallbackMethod createdCallback)
		{
			#if !WINRT_DISABLE_GOOGLE_ADS
			plugin.Native = new AdMob_InterstitialAdPlugin_Native(desc, createdCallback);
			#endif
		}
		#endif
	}

	static class Thread
	{
		public static void Sleep(int milli)
		{
			using (var _event = new ManualResetEvent(false))
			{
				_event.WaitOne(milli);
			}
		}
	}
}
#elif UNITY_WINRT
namespace System.Text.Reign
{
	public class Encoding
	{
		public static Encoding Singleton;

		public delegate Encoding GetEncodingCallbackMethod(int codepage);
		public static GetEncodingCallbackMethod GetEncoding;

		public delegate string GetStringCallbackMathod(byte[] bytes, int index, int count);
		public GetStringCallbackMathod GetString;

		public delegate byte[] GetBytesCallbackMethod(string s);
		public GetBytesCallbackMethod GetBytes;

		public static EncodingASCII ASCII
		{
			get
			{
				if (EncodingASCII.Singleton == null) EncodingASCII.Singleton = new EncodingASCII();
				return EncodingASCII.Singleton;
			}
		}

		public static EncodingUTF8 UTF8
		{
			get
			{
				if (EncodingUTF8.Singleton == null) EncodingUTF8.Singleton = new EncodingUTF8();
				return EncodingUTF8.Singleton;
			}
		}

		static Encoding()
		{
			Singleton = new Encoding();
		}
	}

	public class EncodingASCII : Encoding
	{
		public new static EncodingASCII Singleton;

		public new delegate string GetStringCallbackMathod(byte[] bytes, int index, int count);
		public new GetStringCallbackMathod GetString;

		public new delegate byte[] GetBytesCallbackMethod(string s);
		public new GetBytesCallbackMethod GetBytes;

		public EncodingASCII()
		{
			EncodingASCII.Singleton = this;
		}
	}

	public class EncodingUTF8 : Encoding
	{
		public new static EncodingUTF8 Singleton;

		public new delegate string GetStringCallbackMathod(byte[] bytes, int index, int count);
		public new GetStringCallbackMathod GetString;

		public new delegate byte[] GetBytesCallbackMethod(string s);
		public new GetBytesCallbackMethod GetBytes;

		public EncodingUTF8()
		{
			EncodingUTF8.Singleton = this;
		}
	}

	public class CultureInfo
	{
		public delegate int ANSICodePageCallbackMethod();
		public static ANSICodePageCallbackMethod ANSICodePage;
	}
}
#endif