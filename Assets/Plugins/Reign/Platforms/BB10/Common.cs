#if UNITY_BLACKBERRY
using System;
using System.Runtime.InteropServices;

namespace Reign.Plugin
{
	static class Common
	{
		public const int RTLD_LAZY = 0x0001;
		public const int RTLD_NOW = 0x0002;
		public const int RTLD_GLOBAL = 0x0100;
		public const int RTLD_LOCAL = 0x0200;
	
		// libc
		[DllImport("libc", EntryPoint="getpid")]
		public static extern int getpid();
		
		[DllImport("libc", EntryPoint="dlopen")]
		public static extern IntPtr dlopen(string pathname, int mode);
		
		[DllImport("libc", EntryPoint="dlerror")]
		public static extern IntPtr dlerror();
		
		[DllImport("libc", EntryPoint="dlsym")]
		public static extern IntPtr dlsym(IntPtr handle, string name);

		// device info
		[DllImport("libbps", EntryPoint="deviceinfo_get_details")]
		public static extern int deviceinfo_get_details(ref IntPtr details);

		[DllImport("libbps", EntryPoint="deviceinfo_free_details")]
		public static extern void deviceinfo_free_details(ref IntPtr details);

		[DllImport("libbps", EntryPoint="deviceinfo_details_get_keyboard")]
		public static extern int deviceinfo_details_get_keyboard(IntPtr details);
	
		// system events
		[DllImport("libbps", EntryPoint="bps_get_event")]
		public static extern int bps_get_event(ref IntPtr _event, int timeout);
		
		[DllImport("libbps", EntryPoint="bps_event_get_code")]
		public static extern int bps_event_get_code(IntPtr _event);
		
		[DllImport("libbps", EntryPoint="screen_get_domain")]
		public static extern int screen_get_domain();
		
		[DllImport("libbps", EntryPoint="bps_event_get_domain")]
		public static extern int bps_event_get_domain(IntPtr _event);
		
		// invoke applications
		[DllImport("libbps", EntryPoint="navigator_invoke_invocation_create")]
		public static extern int navigator_invoke_invocation_create(ref IntPtr invoke);
		
		[DllImport("libbps", EntryPoint="navigator_invoke_invocation_set_target")]
		public static extern int navigator_invoke_invocation_set_target(IntPtr invoke, string target);
		
		[DllImport("libbps", EntryPoint="navigator_invoke_invocation_set_action")]
		public static extern int navigator_invoke_invocation_set_action(IntPtr invoke, string action);
		
		[DllImport("libbps", EntryPoint="navigator_invoke_invocation_set_type")]
		public static extern int navigator_invoke_invocation_set_type(IntPtr invoke, string type);

		[DllImport("libbps", EntryPoint="navigator_invoke_invocation_set_data")]
		public static extern int navigator_invoke_invocation_set_data(IntPtr invoke, IntPtr data, int data_length);
		
		[DllImport("libbps", EntryPoint="navigator_invoke_invocation_set_uri")]
		public static extern int navigator_invoke_invocation_set_uri(IntPtr invoke, string uri);
		
		[DllImport("libbps", EntryPoint="navigator_invoke_invocation_send")]
		public static extern int navigator_invoke_invocation_send(IntPtr invoke);
		
		[DllImport("libbps", EntryPoint="navigator_invoke_invocation_destroy")]
		public static extern int navigator_invoke_invocation_destroy(IntPtr invoke);
		
		[DllImport("libbps", EntryPoint="navigator_event_get_card_closed_data")]
		public static extern IntPtr navigator_event_get_card_closed_data(IntPtr _event);
		
		[DllImport("libbps", EntryPoint="navigator_event_get_card_closed_reason")]
		public static extern IntPtr navigator_event_get_card_closed_reason(IntPtr _event);
	}
}
#endif