using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReignPushNotificationServices.Services.Apps
{
	public static class App
	{
		private const string appAPIKey = "9777E040-13D3-465A-BA8A-6D2386CE0A7B";

		public static string Login(string apiKey, string username, string email, string password)
		{
			string response = ResponseTool.CheckAPIKey(apiKey, appAPIKey);
			if (response != null) return response;

			using (var conn = DataManager.CreateConnectionObject())
			{
				conn.Open();
				using (var command = conn.CreateCommand())
				{
					return "TODO";
				}
			}
		}
	}
}