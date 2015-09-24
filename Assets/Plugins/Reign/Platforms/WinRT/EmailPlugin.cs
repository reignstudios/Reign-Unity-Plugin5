#if REIGN_POSTBUILD
using System;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace Reign.Plugin
{
	public class EmailPlugin_Native : IEmailPlugin
	{
		public void Send(string to, string subject, string body)
		{
			sendAsync(to, subject, body);
		}

		#if WINDOWS_PHONE
		private void sendAsync(string to, string subject, string body)
		#else
		private async void sendAsync(string to, string subject, string body)
		#endif
		{
			#if WINDOWS_PHONE
			WinRTPlugin.Dispatcher.BeginInvoke(async delegate()
			#else
			await WinRTPlugin.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async delegate()
			#endif
			{
				string address = string.Format("mailto:?to={0}&subject={1}&body={2}", to, subject, body);
				var mailto = new Uri(address);
				await Windows.System.Launcher.LaunchUriAsync(mailto);
			});
		}
	}
}
#elif UNITY_WINRT
namespace Reign.Plugin
{
	public class EmailPlugin_WinRT : IEmailPlugin
	{
		private IEmailPlugin native;
		public object Native {set{native = (IEmailPlugin)value;}}

		public delegate void InitNativeMethod(EmailPlugin_WinRT plugin);
		public static InitNativeMethod InitNative;

		public EmailPlugin_WinRT()
		{
			InitNative(this);
		}

		public void Send(string to, string subject, string body)
		{
			native.Send(to, subject, body);
		}
	}
}
#endif