using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace ReignScores.Services
{
	public enum ResponseTypes
	{
		Error,
		Succeeded
	}

	public class WebResponse_Score
	{
		[XmlElement("ID")] public string ID;
		[XmlElement("UserID")] public string UserID;
		[XmlElement("Username")] public string Username;
		[XmlElement("Score")] public long Score;
	}

	public class WebResponse_Achievement
	{
		[XmlElement("ID")] public string ID;
		[XmlElement("AchievementID")] public string AchievementID;
		[XmlElement("PercentComplete")] public float PercentComplete;
	}

	public class WebResponse_Game
	{
		[XmlElement("ID")] public string ID;
		[XmlElement("Name")] public string Name;
	}

	[XmlRoot("ClientResponse")]
	public class WebResponse
	{
		[XmlAttribute("Type")] public ResponseTypes Type;
		[XmlElement("ErrorMessage")] public string ErrorMessage;
		[XmlElement("ClientID")] public string ClientID;
		[XmlElement("UserID")] public string UserID;
		[XmlElement("Username")] public string Username;
		[XmlElement("Score")] public List<WebResponse_Score> Scores;
		[XmlElement("Achievement")] public List<WebResponse_Achievement> Achievements;
		[XmlElement("Games")] public List<WebResponse_Game> Games;

		public WebResponse() {}
		public WebResponse(ResponseTypes type)
		{
			this.Type = type;
		}
	}

	public static class ResponseTool
	{
		public static string CheckAPIKey(string apiKey, string requiredAPIKey)
		{
			if (apiKey != requiredAPIKey)
			{
				var response = new WebResponse(ResponseTypes.Error)
				{
					ErrorMessage = "Invalide API Key"
				};

				return ResponseTool.GenerateXML(response);
			}
			
			return null;
		}

		public static string GenerateXML(WebResponse response)
		{
			var xml = new XmlSerializer(typeof(WebResponse));
			using (var writer = new StringWriter())
			{
				xml.Serialize(writer, response);
				return writer.ToString();
			}
		}
	}
}