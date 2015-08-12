using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReignScores.Services.Users
{
	public static class User
	{
		private const string userAPIKey = "CE8E55E1-F383-4F05-9388-5C89F27B7FF2";

		public static string Login(string apiKey, string gameID, string username, string password)
		{
			string response = ResponseTool.CheckAPIKey(apiKey, userAPIKey);
			if (response != null) return response;

			using (var conn = DataManager.CreateConnectionObject())
			{
				conn.Open();
				using (var command = conn.CreateCommand())
				{
					string passwordEncrypted = SecurityManager.Hash(password);
					command.CommandText = string.Format("SELECT ID, Username FROM Users WHERE GameID = '{0}' and Username = '{1}' and Password = '{2}'", gameID, username, passwordEncrypted);
					using (var reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							var webResponse = new WebResponse(ResponseTypes.Succeeded)
							{
								UserID = reader["ID"].ToString(),
								Username = reader["Username"].ToString()
							};
							return ResponseTool.GenerateXML(webResponse);
						}
						else
						{
							var webResponse = new WebResponse(ResponseTypes.Error)
							{
								ErrorMessage = "Failed to find user"
							};
							return ResponseTool.GenerateXML(webResponse);
						}
					}
				}
			}
		}

		public static string ReportScore(string apiKey, string userID, string leaderboardID, string score)
		{
			string response = ResponseTool.CheckAPIKey(apiKey, userAPIKey);
			if (response != null) return response;

			using (var conn = DataManager.CreateConnectionObject())
			{
				conn.Open();
				using (var command = conn.CreateCommand())
				{
					// check if score already exists
					command.CommandText = string.Format("SELECT ID, Score FROM Scores WHERE UserID = '{0}' and LeaderboardID = '{1}'", userID, leaderboardID);
					string id = null;
					long currentScore = 0;
					using (var reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							id = reader["ID"].ToString();
							currentScore = long.Parse(reader["Score"].ToString());
						}
					}

					if (id == null)
					{
						// create score
						string values = string.Format("(NEWID(), '{0}', '{1}', {2}, '{3}')", userID, leaderboardID, score, DateTime.UtcNow);
						command.CommandText = "INSERT INTO Scores (ID, UserID, LeaderboardID, Score, Date) VALUES " + values;
						if (command.ExecuteNonQuery() == 1)
						{
							var webResponse = new WebResponse(ResponseTypes.Succeeded);
							return ResponseTool.GenerateXML(webResponse);
						}
						else
						{
							var webResponse = new WebResponse(ResponseTypes.Error)
							{
								ErrorMessage = "Failed to properly create score"
							};
							return ResponseTool.GenerateXML(webResponse);
						}
					}
					else if (long.Parse(score) > currentScore)
					{
						// update existing score
						command.CommandText = string.Format("UPDATE Scores SET Date = '{0}', Score = {1} WHERE ID = '{2}'", DateTime.UtcNow, score, id);
						if (command.ExecuteNonQuery() == 1)
						{
							var webResponse = new WebResponse(ResponseTypes.Succeeded);
							return ResponseTool.GenerateXML(webResponse);
						}
						else
						{
							var webResponse = new WebResponse(ResponseTypes.Error)
							{
								ErrorMessage = "Failed to properly update score"
							};
							return ResponseTool.GenerateXML(webResponse);
						}
					}
					else
					{
						// current score already higher
						var webResponse = new WebResponse(ResponseTypes.Succeeded);
						return ResponseTool.GenerateXML(webResponse);
					}
				}
			}
		}

		public static string ReportAchievement(string apiKey, string userID, string achievementID, string percentComplete)
		{
			string response = ResponseTool.CheckAPIKey(apiKey, userAPIKey);
			if (response != null) return response;

			using (var conn = DataManager.CreateConnectionObject())
			{
				conn.Open();
				using (var command = conn.CreateCommand())
				{
					// check if achievement already exists
					command.CommandText = string.Format("SELECT ID, PercentComplete FROM Achievements WHERE UserID = '{0}' and AchievementID = '{1}'", userID, achievementID);
					string id = null;
					float currentPercentComplete = 0;
					using (var reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							id = reader["ID"].ToString();
							currentPercentComplete = float.Parse(reader["PercentComplete"].ToString());
						}
					}

					if (id == null)
					{
						// create achievement
						string values = string.Format("(NEWID(), '{0}', '{1}', '{2}', '{3}')", userID, achievementID, percentComplete, DateTime.UtcNow);
						command.CommandText = "INSERT INTO Achievements (ID, UserID, AchievementID, PercentComplete, Date) VALUES " + values;
						if (command.ExecuteNonQuery() == 1)
						{
							var webResponse = new WebResponse(ResponseTypes.Succeeded);
							return ResponseTool.GenerateXML(webResponse);
						}
						else
						{
							var webResponse = new WebResponse(ResponseTypes.Error)
							{
								ErrorMessage = "Failed to properly create achievement"
							};
							return ResponseTool.GenerateXML(webResponse);
						}
					}
					else if (float.Parse(percentComplete) > currentPercentComplete)
					{
						// update existing achievement
						command.CommandText = string.Format("UPDATE Achievements SET Date = '{0}', PercentComplete = {1} WHERE ID = '{2}'", DateTime.UtcNow, percentComplete, id);
						if (command.ExecuteNonQuery() == 1)
						{
							var webResponse = new WebResponse(ResponseTypes.Succeeded);
							return ResponseTool.GenerateXML(webResponse);
						}
						else
						{
							var webResponse = new WebResponse(ResponseTypes.Error)
							{
								ErrorMessage = "Failed to properly update achievement"
							};
							return ResponseTool.GenerateXML(webResponse);
						}
					}
					else
					{
						// current achievement percent already higher
						var webResponse = new WebResponse(ResponseTypes.Succeeded);
						return ResponseTool.GenerateXML(webResponse);
					}
				}
			}
		}

		public static string RequestAchievements(string apiKey, string userID)
		{
			string response = ResponseTool.CheckAPIKey(apiKey, userAPIKey);
			if (response != null) return response;

			using (var conn = DataManager.CreateConnectionObject())
			{
				conn.Open();
				using (var command = conn.CreateCommand())
				{
					command.CommandText = string.Format("SELECT ID, AchievementID, PercentComplete FROM Achievements WHERE UserID = '{0}'", userID);
					using (var reader = command.ExecuteReader())
					{
						var webResponse = new WebResponse(ResponseTypes.Succeeded);
						webResponse.Achievements = new List<WebResponse_Achievement>();
						while (reader.Read())
						{
							var a = new WebResponse_Achievement()
							{
								ID = reader["AchievementID"].ToString(),
								AchievementID = reader["AchievementID"].ToString(),
								PercentComplete = float.Parse(reader["PercentComplete"].ToString())
							};
							webResponse.Achievements.Add(a);
						}

						return ResponseTool.GenerateXML(webResponse);
					}
				}
			}
		}
	}
}
