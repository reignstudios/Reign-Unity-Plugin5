#if UNITY_BLACKBERRY
using System;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Reign.Plugin
{
	public class InAppPurchasePlugin_BB10 : IInAppPurchasePlugin
	{
		public bool IsTrial {get{return false;}}
		public InAppPurchaseID[] InAppIDs {get; private set;}
		private bool testing;
		
		[DllImport("libbps", EntryPoint="paymentservice_request_events")]
		private static extern int paymentservice_request_events(int flags);
		
		[DllImport("libbps", EntryPoint="paymentservice_set_connection_mode")]
		private static extern int paymentservice_set_connection_mode(bool local);
		
		[DllImport("libbps", EntryPoint="paymentservice_purchase_request")]
		private static extern int paymentservice_purchase_request(string digital_good_id, string digital_good_sku, string digital_good_name, string metadata, string app_name, string app_icon, string group_id, ref uint request_id);
		
		[DllImport("libbps", EntryPoint="paymentservice_get_existing_purchases_request")]
		private static extern int paymentservice_get_existing_purchases_request(bool allow_refresh, string group_id, ref uint request_id);
		
		[DllImport("libbps", EntryPoint="paymentservice_get_domain")]
		private static extern int paymentservice_get_domain();
		
		[DllImport("libbps", EntryPoint="paymentservice_event_get_response_code")]
		private static extern int paymentservice_event_get_response_code(IntPtr _event);
		
		[DllImport("libbps", EntryPoint="paymentservice_event_get_digital_good_sku")]
		private static extern IntPtr paymentservice_event_get_digital_good_sku(IntPtr _event, uint index);
		
		[DllImport("libbps", EntryPoint="paymentservice_event_get_error_text")]
		private static extern IntPtr paymentservice_event_get_error_text(IntPtr _event);

		[DllImport("libbps", EntryPoint="paymentservice_get_price")]
		private static extern int paymentservice_get_price(string digital_good_id, string digital_good_sku, string group_id, ref uint request_id);

		[DllImport("libbps", EntryPoint="paymentservice_event_get_price")]
		private static extern IntPtr paymentservice_event_get_price(IntPtr _event);
		
		static InAppPurchasePlugin_BB10()
		{
			paymentservice_request_events(0);
		}

		public InAppPurchasePlugin_BB10(InAppPurchaseDesc desc, InAppPurchaseCreatedCallbackMethod callback)
		{
			bool pass = true;
			try
			{
				InAppIDs = desc.BB10_BlackBerryWorld_InAppIDs;
				
				testing = desc.Testing;
				paymentservice_set_connection_mode(desc.Testing);
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message);
				pass = false;
			}
			
			if (callback != null) callback(pass);
		}

		public void GetProductInfo (InAppPurchaseGetProductInfoCallbackMethod callback)
		{
			if (callback == null) return;

			var priceInfos = new List<InAppPurchaseInfo>();
			foreach (var iapID in InAppIDs)
			{
				int id = Common.getpid();
				string windowGroup = id.ToString();
				uint reqID = 0;
				if (paymentservice_get_price(iapID.ID, iapID.ID, windowGroup, ref reqID) != 0)
				{
					callback(null, false);
					return;
				}

				// wait for event
				while (true)
				{
					IntPtr _event = IntPtr.Zero;
					Common.bps_get_event(ref _event, -1);// wait here for next event
					if (_event != IntPtr.Zero)
					{
						if (paymentservice_get_domain() == Common.bps_event_get_domain(_event))
						{
							if (paymentservice_event_get_response_code(_event) == 0)
							{
								IntPtr pricePtr = paymentservice_event_get_price(_event);
								string price = Marshal.PtrToStringAnsi(pricePtr);
								var product = new InAppPurchaseInfo()
								{
									ID = iapID.ID,
									FormattedPrice = price
								};
								priceInfos.Add(product);
							}
							else
							{
								IntPtr errorPtr = paymentservice_event_get_error_text(_event);
								Debug.LogError(Marshal.PtrToStringAnsi(errorPtr));
								callback(null, false);
								return;
							}

							break;
						}
					}
				}
			}

			callback(priceInfos.ToArray(), priceInfos.Count != 0);
		}

		public void Restore(InAppPurchaseRestoreCallbackMethod restoreCallback)
		{
			if (restoreCallback == null) return;
			
			int id = Common.getpid();
			string windowGroup = id.ToString();
			uint reqID = 0;
			if (paymentservice_get_existing_purchases_request(!testing, windowGroup, ref reqID) != 0)
			{
				foreach (var inAppID in InAppIDs)
				{
					restoreCallback(inAppID.ID, false);
				}
				
				return;
			}

			// wait for event
			var recoveredApps = new List<string>();
			while (true)
			{
				IntPtr _event = IntPtr.Zero;
				Common.bps_get_event(ref _event, -1);// wait here for next event
				if (_event != IntPtr.Zero)
				{
					if (paymentservice_get_domain() == Common.bps_event_get_domain(_event))
					{
						if (paymentservice_event_get_response_code(_event) == 0)
						{
							for (int i = 0; i != InAppIDs.Length; ++i)
							{
								IntPtr inAppIDPtr = paymentservice_event_get_digital_good_sku(_event, (uint)i);
								string inAppID = Marshal.PtrToStringAnsi(inAppIDPtr);
								if (Common.bps_event_get_code(_event) == 0)
								{
									// purchased
									recoveredApps.Add(inAppID);
								}
								else
								{
									// already purchased
									recoveredApps.Add(inAppID);
								}
							}
						}
						else
						{
							IntPtr errorPtr = paymentservice_event_get_error_text(_event);
							Debug.LogError(Marshal.PtrToStringAnsi(errorPtr));
							foreach (var inAppID in InAppIDs)
							{
								restoreCallback(inAppID.ID, false);
							}
							return;
						}
						
						foreach (var inAppID in InAppIDs)
						{
							bool found = false;
							foreach (var app2 in recoveredApps)
							{
								if (inAppID.ID == app2)
								{
									found = true;
									break;
								}
							}
							
							restoreCallback(inAppID.ID, found && inAppID.Type != InAppPurchaseTypes.Consumable);
						}
						
						return;
					}
				}
			}
		}

		public void BuyInApp(string inAppID, InAppPurchaseBuyCallbackMethod purchasedCallback)
		{
			if (purchasedCallback == null) return;

			int id = Common.getpid();
			string windowGroup = id.ToString();
			uint reqID = 0;
			if (paymentservice_purchase_request(inAppID, null, null, null, null, null, windowGroup, ref reqID) != 0)
			{
				purchasedCallback(inAppID, null, false);
				return;
			}
			
			// wait for event
			while (true)
			{
				IntPtr _event = IntPtr.Zero;
				Common.bps_get_event(ref _event, -1);// wait here for next event
				if (_event != IntPtr.Zero)
				{
					if (paymentservice_get_domain() == Common.bps_event_get_domain(_event))
					{
						if (paymentservice_event_get_response_code(_event) == 0)
						{
							if (Common.bps_event_get_code(_event) == 0)
							{
								// purchased
								purchasedCallback(inAppID, null, true);
								return;
							}
							else
							{
								// already purchased
								purchasedCallback(inAppID, null, true);
								return;
							}
						}
						else
						{
							IntPtr errorPtr = paymentservice_event_get_error_text(_event);
							Debug.LogError(Marshal.PtrToStringAnsi(errorPtr));
							purchasedCallback(inAppID, null, false);
							return;
						}
					}
				}
			}
		}
		
		public void Update()
		{
			// do nothing...
		}
	}
}
#endif