#if UNITY_STANDALONE_WIN
using System;
using System.Runtime.InteropServices;

namespace Reign.Plugin
{
	public class MessageBoxPlugin_Win32 : IMessageBoxPlugin
	{
		[DllImport("User32.dll", EntryPoint="MessageBox")]
		private extern static int MessageBox(IntPtr hWnd, string lpText, string lpCaption, uint uType);

		private const uint MB_OK = (uint)0x00000000L;
		private const uint MB_OKCANCEL = (uint)0x00000001L;
		private const int IDCANCEL = 2;

		public void Show(string title, string message, MessageBoxTypes type, MessageBoxCallback callback)
		{
			if (type == MessageBoxTypes.Ok)
			{
				MessageBox(IntPtr.Zero, message, title, MB_OK);
				if (callback != null) callback(MessageBoxResult.Ok);
			}
			else
			{
				int result = MessageBox(IntPtr.Zero, message, title, MB_OKCANCEL);
				if (callback != null) callback(result != IDCANCEL ? MessageBoxResult.Ok : MessageBoxResult.Cancel);
			}
		}
		
		public void Update()
		{
			// do nothing...
		}
	}
}
#endif