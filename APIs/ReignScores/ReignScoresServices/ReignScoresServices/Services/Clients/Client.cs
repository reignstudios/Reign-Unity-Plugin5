using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReignScores.Services.Clients
{
	public static class Client
	{
		private const string clientAPIKey = "8AC57612-B355-4F24-9B77-8C020B57E361";

		public static string Login(string apiKey, string username, string password)
		{
			string response = ResponseTool.CheckAPIKey(apiKey, clientAPIKey);
			if (response != null) return response;

			using (var conn = DataManager.CreateConnectionObject())
			{
				conn.Open();
				using (var command = conn.CreateCommand())
				{
					string passwordEncrypted = SecurityManager.Hash(password);
					command.CommandText = string.Format("SELECT ID FROM Clients WHERE Username = '{0}' and Password = '{1}'", username, passwordEncrypted);
					using (var reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							var webResponse = new WebResponse(ResponseTypes.Succeeded)
							{
								ClientID = reader["ID"].ToString()
							};
							return ResponseTool.GenerateXML(webResponse);
						}
						else
						{
							var webResponse = new WebResponse(ResponseTypes.Error)
							{
								ErrorMessage = "Failed to find client"
							};
							return ResponseTool.GenerateXML(webResponse);
						}
					}
				}
			}
		}

		public static string CreateGame(string apiKey, string clientID, string name)
		{
			string response = ResponseTool.CheckAPIKey(apiKey, clientAPIKey);
			if (response != null) return response;

			using (var conn = DataManager.CreateConnectionObject())
			{
				conn.Open();
				using (var command = conn.CreateCommand())
				{
					// make sure name doesn't already exist
					command.CommandText = string.Format("SELECT ID FROM Games WHERE Name = '{0}'", name);
					using (var reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							var webResponse = new WebResponse(ResponseTypes.Error)
							{
								ErrorMessage = "Game Name already exists"
							};
							return ResponseTool.GenerateXML(webResponse);
						}
					}

					// create account
					string values = string.Format("(NEWID(), '{0}', '{1}', '{2}')", clientID, name, DateTime.UtcNow);
					command.CommandText = "INSERT INTO Games (ID, ClientID, Name, DateCreated) VALUES " + values;
					if (command.ExecuteNonQuery() == 1)
					{
						var webResponse = new WebResponse(ResponseTypes.Succeeded);
						return ResponseTool.GenerateXML(webResponse);
					}
					else
					{
						var webResponse = new WebResponse(ResponseTypes.Error)
						{
							ErrorMessage = "Failed to properly create game"
						};
						return ResponseTool.GenerateXML(webResponse);
					}
				}
			}
		}

		public static string DeleteGame(string apiKey, string gameID)
		{
			string response = ResponseTool.CheckAPIKey(apiKey, clientAPIKey);
			if (response != null) return response;

			using (var conn = DataManager.CreateConnectionObject())
			{
				conn.Open();
				using (var command = conn.CreateCommand())
				{
					// get all users
					command.CommandText = string.Format("SELECT ID FROM Users WHERE GameID = '{0}'", gameID);
					var userIDs = new List<Guid>();
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							userIDs.Add(new Guid(reader["ID"].ToString()));
						}
					}

					// delete all user scores & achievemetns
					foreach (var userID in userIDs)
					{
						// scores
						command.CommandText = string.Format("DELETE FROM Scores WHERE UserID = '{0}'", userID);
						command.ExecuteNonQuery();

						// achievements
						command.CommandText = string.Format("DELETE FROM Achievements WHERE UserID = '{0}'", userID);
						command.ExecuteNonQuery();
					}

					// delete users
					command.CommandText = string.Format("DELETE FROM Users WHERE GameID = '{0}'", gameID);
					command.ExecuteNonQuery();

					// delete game
					command.CommandText = string.Format("DELETE FROM Games WHERE ID = '{0}'", gameID);
					var webResponse = new WebResponse(command.ExecuteNonQuery() != 0 ? ResponseTypes.Succeeded : ResponseTypes.Error);
					return ResponseTool.GenerateXML(webResponse);
				}
			}
		}

		public static string GetGameList(string apiKey, string clientID)
		{
			string response = ResponseTool.CheckAPIKey(apiKey, clientAPIKey);
			if (response != null) return response;

			using (var conn = DataManager.CreateConnectionObject())
			{
				conn.Open();
				using (var command = conn.CreateCommand())
				{
					var webResponse = new WebResponse(ResponseTypes.Succeeded);
					webResponse.Games = new List<WebResponse_Game>();
					command.CommandText = string.Format("SELECT ID, Name FROM Games WHERE ClientID = '{0}'", clientID);
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							var game = new WebResponse_Game()
							{
								ID = reader["ID"].ToString(),
								Name = reader["Name"].ToString()
							};
							webResponse.Games.Add(game);
						}
					}

					return ResponseTool.GenerateXML(webResponse);
				}
			}
		}
	}
}