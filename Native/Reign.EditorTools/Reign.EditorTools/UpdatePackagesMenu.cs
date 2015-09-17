using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;

namespace Reign.EditorTools
{
	public class UpdatePackagesMenu
	{
		private const string checkURL = "http://www.reign-studios-services.com/ReignUnityPluginUpdater/5/WebPluginPackageValidateOwnership.aspx";
		private const string downloadURL = "http://www.reign-studios-services.com/ReignUnityPluginUpdater/5/WebPluginPackageDownload.aspx";

		[MenuItem("Edit/Reign/Check for updates")]
		static void CheckForUpdates()
		{
			string version;
			using (var stream = new FileStream(Application.dataPath + "/Plugins/Reign/VersionInfo/ReignVersionCheck", FileMode.Open, FileAccess.Read, FileShare.None))
			using (var reader = new StreamReader(stream))
			{
				version = reader.ReadLine();
			}

			using (var www = new WWW("http://www.reign-studios-services.com/ReignUnityPluginUpdater/5/WebPluginCheckForUpdates.aspx?Version=" + version))
			{
				while (!www.isDone) System.Threading.Thread.Sleep(1);
				if (!string.IsNullOrEmpty(www.error))
				{
					Debug.LogError("Check for updates Error: " + www.error);
					return;
				}

				if (www.text == "CurrentVersion")
				{
					Debug.Log("You are up to date!");
					EditorUtility.DisplayDialog("Current Version", "You are up to date!\nVersion: " + version, "OK");
				}
				else if (www.text == "OldVersion")
				{
					Debug.LogError("Out of Date! Please download a updated Unitypackage!");
					EditorUtility.DisplayDialog("Current Version", "Out of Date!\nPlease download a updated Unitypackage!", "OK");
				}
				else
				{
					Debug.LogError("Invalid server response!");
				}
			}
		}

		[MenuItem("Edit/Reign/Download UnityPackage Updates/Download WinRT package")]
		static void DownloadWinRT()
		{
			string versionCheck = Application.dataPath+"/Plugins/Reign/VersionInfo/ReignVersionCheck_WinRT";
			if (File.Exists(versionCheck))
			{
				using (var stream = new FileStream(versionCheck, FileMode.Open, FileAccess.Read, FileShare.Read))
				using (var reader = new StreamReader(stream))
				{
					string versionSyncCode = reader.ReadLine();
					string key = reader.ReadLine();
					if (validateKey(key)) downloadFile(key, "WinRT");
				}
			}
			else
			{
				Debug.LogError("You dont own or have not installed the WinRT Reign Unity package first.");
			}
		}

		[MenuItem("Edit/Reign/Download UnityPackage Updates/Download BB10 package")]
		static void DownloadBB10()
		{
			string versionCheck = Application.dataPath+"/Plugins/Reign/VersionInfo/ReignVersionCheck_BB10";
			if (File.Exists(versionCheck))
			{
				using (var stream = new FileStream(versionCheck, FileMode.Open, FileAccess.Read, FileShare.Read))
				using (var reader = new StreamReader(stream))
				{
					string versionSyncCode = reader.ReadLine();
					string key = reader.ReadLine();
					if (validateKey(key)) downloadFile(key, "BB10");
				}
			}
			else
			{
				Debug.LogError("You dont own or have not installed the BB10 Reign Unity package first.");
			}
		}

		[MenuItem("Edit/Reign/Download UnityPackage Updates/Download iOS package")]
		static void DownloadIOS()
		{
			string versionCheck = Application.dataPath+"/Plugins/Reign/VersionInfo/ReignVersionCheck_iOS";
			if (File.Exists(versionCheck))
			{
				using (var stream = new FileStream(versionCheck, FileMode.Open, FileAccess.Read, FileShare.Read))
				using (var reader = new StreamReader(stream))
				{
					string versionSyncCode = reader.ReadLine();
					string key = reader.ReadLine();
					if (validateKey(key)) downloadFile(key, "iOS");
				}
			}
			else
			{
				Debug.LogError("You dont own or have not installed the iOS Reign Unity package first.");
			}
		}

		[MenuItem("Edit/Reign/Download UnityPackage Updates/Download Android package")]
		static void DownloadAndroid()
		{
			string versionCheck = Application.dataPath+"/Plugins/Reign/VersionInfo/ReignVersionCheck_Android";
			if (File.Exists(versionCheck))
			{
				using (var stream = new FileStream(versionCheck, FileMode.Open, FileAccess.Read, FileShare.Read))
				using (var reader = new StreamReader(stream))
				{
					string versionSyncCode = reader.ReadLine();
					string key = reader.ReadLine();
					if (validateKey(key)) downloadFile(key, "Android");
				}
			}
			else
			{
				Debug.LogError("You dont own or have not installed the Android Reign Unity package first.");
			}
		}

		[MenuItem("Edit/Reign/Download UnityPackage Updates/Download Ultimate package")]
		static void DownloadUltimate()
		{
			int validCount = 0;

			// WinRT = Win8, WP8, Win10, WP10
			string versionCheck = Application.dataPath+"/Plugins/Reign/VersionInfo/ReignVersionCheck_WinRT";
			if (File.Exists(versionCheck))
			{
				using (var stream = new FileStream(versionCheck, FileMode.Open, FileAccess.Read, FileShare.Read))
				using (var reader = new StreamReader(stream))
				{
					string versionSyncCode = reader.ReadLine();
					string key = reader.ReadLine();
					if (validateKey(key)) ++validCount;
				}
			}
			else
			{
				Debug.LogError("You dont own or have not installed the WinRT Reign Unity package first.");
			}

			// bb10
			versionCheck = Application.dataPath+"/Plugins/Reign/VersionInfo/ReignVersionCheck_BB10";
			if (File.Exists(versionCheck))
			{
				using (var stream = new FileStream(versionCheck, FileMode.Open, FileAccess.Read, FileShare.Read))
				using (var reader = new StreamReader(stream))
				{
					string versionSyncCode = reader.ReadLine();
					string key = reader.ReadLine();
					if (validateKey(key)) ++validCount;
				}
			}
			else
			{
				Debug.LogError("You dont own or have not installed the BB10 Reign Unity package first.");
			}

			// ios
			versionCheck = Application.dataPath+"/Plugins/Reign/VersionInfo/ReignVersionCheck_iOS";
			if (File.Exists(versionCheck))
			{
				using (var stream = new FileStream(versionCheck, FileMode.Open, FileAccess.Read, FileShare.Read))
				using (var reader = new StreamReader(stream))
				{
					string versionSyncCode = reader.ReadLine();
					string key = reader.ReadLine();
					if (validateKey(key)) ++validCount;
				}
			}
			else
			{
				Debug.LogError("You dont own or have not installed the iOS Reign Unity package first.");
			}

			// android
			versionCheck = Application.dataPath+"/Plugins/Reign/VersionInfo/ReignVersionCheck_Android";
			if (File.Exists(versionCheck))
			{
				using (var stream = new FileStream(versionCheck, FileMode.Open, FileAccess.Read, FileShare.Read))
				using (var reader = new StreamReader(stream))
				{
					string versionSyncCode = reader.ReadLine();
					string key = reader.ReadLine();
					if (validateKey(key)) ++validCount;
				}
			}
			else
			{
				Debug.LogError("You dont own or have not installed the Android Reign Unity package first.");
			}

			// download
			if (validCount == 4) downloadFile("fbaff080-cf8b-426e-bda7-ca4040c7665d", "Ultimate");
			else Debug.LogError("You dont own or have not installed the required packages first.");
		}

		private static bool validateKey(string key)
		{
			string url = checkURL;
			url += "?Key=" + key;
			using (var www = new WWW(url))
			{
				while (!www.isDone) System.Threading.Thread.Sleep(1);
				if (!string.IsNullOrEmpty(www.error))
				{
					Debug.LogError("Update Download Error: " + www.error);
					return false;
				}

				// make sure we are authenticated
				if (www.text == "InvalidKey")
				{
					Debug.LogError("Key is invalid.");
					return false;
				}
				else if (www.text == "ValidKey")
				{
					return true;
				}
			}

			return false;
		}

		private static void downloadFile(string key, string platformName)
		{
			string fileName = EditorUtility.SaveFilePanel("Save .unitypackage file", "", "ReignUnityPackage_"+platformName, "unitypackage");
			EditorUtility.DisplayDialog("Downloading Alert", "NOTE: Your Unity Editor will be locked until the download is done!\nUnity is not frozen just give it some time.", "Ok");
			if (string.IsNullOrEmpty(fileName)) return;

			string url = downloadURL;
			url += "?Key=" + key;
			using (var www = new WWW(url))
			{
				while (!www.isDone) System.Threading.Thread.Sleep(1);
				if (!string.IsNullOrEmpty(www.error))
				{
					Debug.LogError("Package Download Error: " + www.error);
					return;
				}

				// make sure we are authenticated
				if (www.text == "NotAuthenticated")
				{
					Debug.LogError("Key not authenticated.");
					return;
				}

				// save file to disk
				var data = www.bytes;
				if (data != null)
				{
					Debug.Log("File size: " + data.Length);
					using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
					{
						stream.Write(data, 0, data.Length);
					}
				}
				else
				{
					Debug.Log("Data Null for file");
				}
			}
		}
	}
}
