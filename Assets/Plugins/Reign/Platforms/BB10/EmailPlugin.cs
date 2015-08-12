#if UNITY_BLACKBERRY
using System;
using System.Runtime.InteropServices;

namespace Reign.Plugin
{
	public class EmailPlugin_BB10 : IEmailPlugin
	{
		private IntPtr invoke;
		
		public void Send(string to, string subject, string body)
		{
			if (Common.navigator_invoke_invocation_create(ref invoke) != 0) return;
			if (Common.navigator_invoke_invocation_set_target(invoke, "sys.pim.uib.email.hybridcomposer") != 0)
			{
				dispose();
				return;
			}
			
			if (Common.navigator_invoke_invocation_set_action(invoke, "bb.action.OPEN, bb.action.SENDEMAIL") != 0)
			{
				dispose();
				return;
			}
			
			if (Common.navigator_invoke_invocation_set_uri(invoke, string.Format("mailto:{0}?subject={1}&body={2}", to, subject, body)) != 0)
			{
				dispose();
				return;
			}
			
			Common.navigator_invoke_invocation_send(invoke);
			dispose();
		}
		
		private void dispose()
		{
			Common.navigator_invoke_invocation_destroy(invoke);
		}
	}
}
#endif