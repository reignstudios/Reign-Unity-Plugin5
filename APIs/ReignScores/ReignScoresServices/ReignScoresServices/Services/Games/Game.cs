using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReignScores.Services.Games
{
	public static class Game
	{
		private const string gameAPIKey = "04E0676D-AAF8-4836-A584-DE0C1D618D84";

		public static string CreateUser(string apiKey, string gameID, string username, string password)
		{
			string response = ResponseTool.CheckAPIKey(apiKey, gameAPIKey);
			if (response != null) return response;

			using (var conn = DataManager.CreateConnectionObject())
			{
				conn.Open();
				using (var command = conn.CreateCommand())
				{
					// make sure username doesn't already exist
					command.CommandText = string.Format("SELECT ID FROM Users WHERE GameID = '{0}' and Username = '{1}'", gameID, username);
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
					var userID = Guid.NewGuid();
					string values = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}')", userID, gameID, username, passwordEncrypted, DateTime.UtcNow);
					command.CommandText = "INSERT INTO Users (ID, GameID, Username, Password, DateCreated) VALUES " + values;
					if (command.ExecuteNonQuery() == 1)
					{
						var webResponse = new WebResponse(ResponseTypes.Succeeded)
						{
							UserID = userID.ToString(),
							Username = username
						};
						return ResponseTool.GenerateXML(webResponse);
					}
					else
					{
						var webResponse = new WebResponse(ResponseTypes.Error)
						{
							ErrorMessage = "Failed to properly create user"
						};
						return ResponseTool.GenerateXML(webResponse);
					}
				}
			}
		}

		public static string RequestScores(string apiKey, string leaderboardID, string offset, string range, string sortOrder)
		{
			string response = ResponseTool.CheckAPIKey(apiKey, gameAPIKey);
			if (response != null) return response;

			using (var conn = DataManager.CreateConnectionObject())
			{
				conn.Open();
				using (var command = conn.CreateCommand())
				{
					var webResponse = new WebResponse(ResponseTypes.Succeeded);
					webResponse.Scores = new List<WebResponse_Score>();

					// get all leaderboard usersIDs
					command.CommandText = string.Format("SELECT ID, UserID, Score FROM Scores WHERE LeaderboardID = '{0}' ORDER BY Score {3} OFFSET {1} ROWS FETCH NEXT {2} ROWS ONLY", leaderboardID, offset, range, sortOrder == "Ascending" ? "ASC" : "DESC");
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							webResponse.Scores.Add(new WebResponse_Score()
							{
								ID = reader["ID"].ToString(),
								UserID = reader["UserID"].ToString(),
								Score = long.Parse(reader["Score"].ToString())
							});
						}
					}

					// get all usernames for IDs
					foreach (var score in webResponse.Scores)
					{
						command.CommandText = string.Format("SELECT Username FROM Users WHERE ID = '{0}'", score.UserID);
						using (var reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								score.Username = reader["Username"].ToString();
							}
						}
					}

					return ResponseTool.GenerateXML(webResponse);
				}
			}
		}
	}
}