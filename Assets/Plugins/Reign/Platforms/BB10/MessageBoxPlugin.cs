#if UNITY_BLACKBERRY
using System;
using System.Runtime.InteropServices;

namespace Reign.Plugin
{
	public class MessageBoxPlugin_BB10 : IMessageBoxPlugin
	{
		private IntPtr dialog;
	
		[DllImport("libbps", EntryPoint="dialog_request_events")]
		private static extern int dialog_request_events(int flags);
		
		[DllImport("libbps", EntryPoint="dialog_get_domain")]
		private static extern int dialog_get_domain();
	
		[DllImport("libbps", EntryPoint="dialog_create_alert")]
		private static extern int dialog_create_alert(ref IntPtr dialog);
		
		[DllImport("libbps", EntryPoint="dialog_set_title_text")]
		private static extern int dialog_set_title_text(IntPtr dialog, string text);
		
		[DllImport("libbps", EntryPoint="dialog_set_alert_message_text")]
		private static extern int dialog_set_alert_message_text(IntPtr dialog, string text);
		
		[DllImport("libbps", EntryPoint="dialog_add_button")]
		private static extern int dialog_add_button(IntPtr dialog, string label, bool enabled, string button_context, bool visible);
		
		[DllImport("libbps", EntryPoint="dialog_show")]
		private static extern int dialog_show(IntPtr dialog);
		
		[DllImport("libbps", EntryPoint="dialog_destroy")]
		private static extern int dialog_destroy(IntPtr dialog);
		
		[DllImport("libbps", EntryPoint="dialog_event_get_selected_context")]
		private static extern IntPtr dialog_event_get_selected_context(IntPtr _event);
	
		public void Show(string title, string message, MessageBoxTypes type, MessageBoxOptions options, MessageBoxCallback callback)
		{
			const string okContextID = "okButton", cancelContextID = "cancelButton";
		
			dialog_request_events(0);
			if (dialog_create_alert(ref dialog) != 0) return;
			if (dialog_set_title_text(dialog, title) != 0)
			{
				dispose();
				return;
			}
			
			if (dialog_set_alert_message_text(dialog, message) != 0)
			{
				dispose();
				return;
			}
			
			if (dialog_add_button(dialog, options.OkButtonName, true, okContextID, true) != 0)
			{
				dispose();
				return;
			}
			
			if (type == MessageBoxTypes.OkCancel && dialog_add_button(dialog, options.CancelButtonText, true, cancelContextID, true) != 0)
			{
				dispose();
				return;
			}
			
			if (dialog_show(dialog) != 0)
			{
				dispose();
				return;
			}
			
			// wait for messge box event
			while (true)
			{
				IntPtr _event = IntPtr.Zero;
				Common.bps_get_event(ref _event, -1);// wait here for next event
				if (_event != IntPtr.Zero)
				{
					if (dialog_get_domain() == Common.bps_event_get_domain(_event))
					{
						var context = dialog_event_get_selected_context(_event);
						if (context != IntPtr.Zero)
						{
							if (Marshal.PtrToStringAnsi(context) == okContextID)
							{
								if (callback != null) callback(MessageBoxResult.Ok);
								break;
							}
							
							if (Marshal.PtrToStringAnsi(context) == cancelContextID)
							{
								if (callback != null) callback(MessageBoxResult.Cancel);
								break;
							}
						}
					}
				}
			}
			
			dispose();
		}
		
		private void dispose()
		{
			dialog_destroy(dialog);
		}
		
		public void Update()
		{
			// do nothing...
		}
	}
}
#endif