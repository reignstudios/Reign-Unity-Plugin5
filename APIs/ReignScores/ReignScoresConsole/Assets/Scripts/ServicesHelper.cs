using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace XML
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
		[XmlElement("Score")] public int Score;
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
}

public enum ServiceTypes
{
	Managers,
	Clients,
	Games,
	Users
}

public class ServicesHelper : MonoBehaviour
{
	public delegate void CallbackMethod(bool succeeded, XML.WebResponse response);

	public GameObject ProcessingCanvas;
	public string ServicesURL = "http://localhost:5537/Services/";
	public string Manager_API_Key = "9777E040-13D3-465A-BA8A-6D2386CE0A7B";
	public string Client_API_Key = "8AC57612-B355-4F24-9B77-8C020B57E361";
	public string Game_API_Key = "04E0676D-AAF8-4836-A584-DE0C1D618D84";
	public string User_API_Key = "CE8E55E1-F383-4F05-9388-5C89F27B7FF2";

	public static ServicesHelper Singleton;
	void Start()
	{
		Singleton = this;
	}

	private static string convertType(ServiceTypes type)
	{
		switch (type)
		{
			case ServiceTypes.Managers: return "Managers";
			case ServiceTypes.Clients: return "Clients";
			case ServiceTypes.Games: return "Games";
			case ServiceTypes.Users: return "Users";
			default: throw new Exception("Invalid type: " + type);
		}
	}

	private static string convertAPI_Key(ServiceTypes type)
	{
		switch (type)
		{
			case ServiceTypes.Managers: return Singleton.Manager_API_Key;
			case ServiceTypes.Clients: return Singleton.Client_API_Key;
			case ServiceTypes.Games: return Singleton.Game_API_Key;
			case ServiceTypes.Users: return Singleton.User_API_Key;
			default: throw new Exception("Invalid type: " + type);
		}
	}

	private static XML.WebResponse generateResponse(string response)
	{
		try
		{
			var xml = new XmlSerializer(typeof(XML.WebResponse));
			using (var reader = new StringReader(response))
			{
				return xml.Deserialize(reader) as XML.WebResponse;
			}
		}
		catch (Exception e)
		{
			Debug.LogError("generateResponse Failed: " + e.Message);
		}

		return null;
	}

	public static void InvokeServiceMethod(ServiceTypes type, string method, CallbackMethod callback, params string[] args)
	{
		if (callback != null)
		{
			string url = string.Format("{0}{1}/{2}.cshtml", Singleton.ServicesURL, convertType(type), method);
			Debug.Log("Invoking URL: " + url);
			Singleton.StartCoroutine(Singleton.invokeWebMethod(convertAPI_Key(type), url, callback, args));
		}
	}

	private IEnumerator invokeWebMethod(string api_key, string url, CallbackMethod callback, params string[] args)
	{
		ProcessingCanvas.SetActive(true);

		// add form data
		var form = new WWWForm();
		form.AddField("api_key", api_key);
		foreach (var arg in args)
		{
			var values = arg.Split('=');
			form.AddField(values[0], values[1]);
		}

		// invoke http method
		var www = new WWW(url, form);
		yield return www;

		// check for server errors
		if (!string.IsNullOrEmpty(www.error))
		{
			Debug.LogError(www.error);
			callback(false, null);
			ProcessingCanvas.SetActive(false);
			yield break;
		}

		// process response
		var response = generateResponse(www.text);
		if (response != null)
		{
			if (response.Type == XML.ResponseTypes.Error)
			{
				Debug.LogError("Failed: " + response.ErrorMessage);
				callback(false, null);
				ProcessingCanvas.SetActive(false);
				yield break;
			}

			callback(true, response);
		}
		else
		{
			Debug.LogError("response is null");
			callback(false, null);
			ProcessingCanvas.SetActive(false);
			yield break;
		}

		ProcessingCanvas.SetActive(false);
	}
}
