#if UNITY_STANDALONE_OSX
using System;
using UnityEngine;
using System.Runtime.InteropServices;

namespace Reign.Plugin
{
	public class MessageBoxPlugin_OSX : IMessageBoxPlugin
	{
		[DllImport("ReignNative", EntryPoint="MessageBoxInit")]
		private extern static void MessageBoxInit();

		[DllImport("ReignNative", EntryPoint="MessageBoxShow")]
		private extern static int MessageBoxShow(string title, string message, int type);

		public MessageBoxPlugin_OSX()
		{
			MessageBoxInit();
		}

		public void Show(string title, string message, MessageBoxTypes type, MessageBoxOptions options, MessageBoxCallback callback)
		{
			if (type == MessageBoxTypes.Ok)
			{
				MessageBoxShow(title, message, 0);
				if (callback != null) callback(MessageBoxResult.Ok);
			}
			else
			{
				int result = MessageBoxShow(title, message, 1);
				Debug.Log("VALUE: " + result);
				if (callback != null) callback(result == 1 ? MessageBoxResult.Ok : MessageBoxResult.Cancel);
			}
		}
		
		public void Update()
		{
			// do nothing...
		}
	}
}
#endif