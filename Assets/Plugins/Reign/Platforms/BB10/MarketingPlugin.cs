#if UNITY_BLACKBERRY
using System;

namespace Reign.Plugin
{
    public class MarketingPlugin_BB10 : IIMarketingPlugin
    {
		private void invokeStore(string appID)
		{
			IntPtr invoke = IntPtr.Zero;
		
			if (Common.navigator_invoke_invocation_create(ref invoke) != 0) return;
			if (Common.navigator_invoke_invocation_set_target(invoke, "sys.appworld") != 0)// sys.filesaver.target << use for file save dialog
			{
				Common.navigator_invoke_invocation_destroy(invoke);
				return;
			}
			
			if (Common.navigator_invoke_invocation_set_action(invoke, "bb.action.OPEN") != 0)
			{
				Common.navigator_invoke_invocation_destroy(invoke);
				return;
			}
			
			if (Common.navigator_invoke_invocation_set_uri(invoke, "appworld://content/" + appID) != 0)
			{
				Common.navigator_invoke_invocation_destroy(invoke);
				return;
			}
			
			if (Common.navigator_invoke_invocation_send(invoke) != 0)
			{
				Common.navigator_invoke_invocation_destroy(invoke);
				return;
			}
			
			Common.navigator_invoke_invocation_destroy(invoke);
		}
    
    	public void OpenStore(MarketingDesc desc)
		{
			invokeStore(desc.BB10_AppID);
		}
		
		public void OpenStoreForReview(MarketingDesc desc)
		{
			invokeStore(desc.BB10_AppID);
		}
    }
}
#endif