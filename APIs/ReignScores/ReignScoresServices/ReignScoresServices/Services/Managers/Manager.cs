using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReignScores.Services.Managers
{
	public static class Manager
	{
		private const string managerAPIKey = "9777E040-13D3-465A-BA8A-6D2386CE0A7B";

		public static string CreateClient(string apiKey, string username, string email, string password)
		{
			string response = ResponseTool.CheckAPIKey(apiKey, managerAPIKey);
			if (response != null) return response;

			using (var conn = DataManager.CreateConnectionObject())
			{
				conn.Open();
				using (var command = conn.CreateCommand())
				{
					// make sure username doesn't already exist
					command.CommandText = string.Format("SELECT ID FROM Clients WHERE Username = '{0}'", username);
					using (var reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							var webResponse = new WebResponse(ResponseTypes.Error)
							{
								ErrorMessage = "Username already exists"
							};
							return ResponseTool.GenerateXML(webResponse);
						}
					}

					// create account
					string passwordEncrypted = SecurityManager.Hash(password);
					string values = string.Format("(NEWID(), '{0}', '{1}', '{2}', '{3}')", username, email, passwordEncrypted, DateTime.UtcNow);
					command.CommandText = "INSERT INTO Clients (ID, Username, Email, Password, DateCreated) VALUES " + values;
					if (command.ExecuteNonQuery() == 1)
					{
						var webResponse = new WebResponse(ResponseTypes.Succeeded);
						return ResponseTool.GenerateXML(webResponse);
					}
					else
					{
						var webResponse = new WebResponse(ResponseTypes.Error)
						{
							ErrorMessage = "Failed to properly create client"
						};
						return ResponseTool.GenerateXML(webResponse);
					}
				}
			}
		}
	}
}