// -------------------------------------------------------
//  Created by Andrew Witte.
//  Copyright (c) 2013 Reign-Studios. All rights reserved.
// -------------------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.IO;

public static class ExportPlugins
{
	private static void zipPath(string srcPath, string dstFile)
	{
		var start = new System.Diagnostics.ProcessStartInfo();
		start.Arguments = srcPath + " " + dstFile;
		start.FileName = Application.dataPath + "/Editor/Reign/PublisherOnly/ZipCompressor.exe";
		start.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
		using (var proc = System.Diagnostics.Process.Start(start))
		{
			proc.WaitForExit();

			var exitCode = proc.ExitCode;
			if (exitCode != 0) Debug.LogError("Exit Code: " + exitCode);
		}
	}

	[MenuItem("Reign Dev/Prepare Release")]
	static void PrepareRelease()
	{
		// sync version files
		SyncVersionFiles();

		// reset review settings
		using (var stream = new FileStream(Application.dataPath+"/Editor/Reign/ReviewSettings", FileMode.Create, FileAccess.Write, FileShare.None))
		using (var writer = new StreamWriter(stream))
		{
			writer.Write("0");
		}

		// generate clean files list
		using (var stream = new FileStream(Application.dataPath+"/Editor/Reign/CleanSettings", FileMode.Create, FileAccess.Write, FileShare.None))
		using (var writer = new StreamWriter(stream))
		{
			globalFilesAdded = false;
			var files = getAllFiles(null);
			foreach (var file in files)
			{
				if (Path.GetExtension(file) == ".DS_Store") continue;
				string value = file.Remove(0, Application.dataPath.Length).Replace('\\', '/');
				switch (value)
				{
					case "/Editor/Reign/Reign.EditorTools.dll":
					case "/Plugins/Reign/VersionInfo/ReignVersionCheck":
					case "/Plugins/Reign/VersionInfo/ReignVersionCheck_WinRT":
					case "/Plugins/Reign/VersionInfo/ReignVersionCheck_BB10":
					case "/Plugins/Reign/VersionInfo/ReignVersionCheck_Android":
					case "/Plugins/Reign/VersionInfo/ReignVersionCheck_iOS":
						break;

					default:
						writer.WriteLine(value);
						break;
				}
			}
		}

		// zip external source code
		string rootPath = Application.dataPath.Replace("Assets", "");
		zipPath("src=" + rootPath + @"\APIs\ReignScores", "dst=" + Application.dataPath + "/Editor/Reign/ReignScores.zip");

		AssetDatabase.Refresh();
		Debug.Log("Prepare Done!");
	}

	[MenuItem("Reign Dev/Sync Version Files")]
	static void SyncVersionFiles()
	{
		string path = Application.dataPath + "/Plugins/Reign/VersionInfo/";
		string newVersion;
		string key = readFileVersion(convertPathToPlatform(path+"ReignVersionCheck"), out newVersion);

		string oldVersion;
		key = readFileVersion(convertPathToPlatform(path+"ReignVersionCheck_WinRT"), out oldVersion);
		writeFileVersion(convertPathToPlatform(path+"ReignVersionCheck_WinRT"), newVersion, key);

		key = readFileVersion(convertPathToPlatform(path+"ReignVersionCheck_BB10"), out oldVersion);
		writeFileVersion(convertPathToPlatform(path+"ReignVersionCheck_BB10"), newVersion, key);

		key = readFileVersion(convertPathToPlatform(path+"ReignVersionCheck_Android"), out oldVersion);
		writeFileVersion(convertPathToPlatform(path+"ReignVersionCheck_Android"), newVersion, key);

		key = readFileVersion(convertPathToPlatform(path+"ReignVersionCheck_iOS"), out oldVersion);
		writeFileVersion(convertPathToPlatform(path+"ReignVersionCheck_iOS"), newVersion, key);
	}

	private static string readFileVersion(string fileName, out string version)
	{
		using (var file = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None))
		using (var reader = new StreamReader(file))
		{
			version = reader.ReadLine();
			return reader.ReadLine();
		}
	}

	private static void writeFileVersion(string fileName, string version, string key)
	{
		using (var file = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
		using (var writer = new StreamWriter(file))
		{
			writer.WriteLine(version);
			writer.Write(key);
		}
	}

	// ========================================================
	// packaging code
	// ========================================================
	private static string convertPathToPlatform(string path)
	{
		return path.Replace('\\', '/');
	}
	
	private static void getFilesInPath(string path, List<string> files)
	{
		// search sub directories
		foreach (var dir in Directory.GetDirectories(path))
		{
			if (Path.GetFileName(dir) != "PublisherOnly") getFilesInPath(dir, files);
		}

		// add all files but .meta files
		foreach (var file in Directory.GetFiles(path))
		{
			if (Path.GetExtension(file) != ".meta") files.Add(file);
		}
	}

	private static bool globalFilesAdded;
	private static List<string> getGlobalFiles(List<string> files)
	{
		if (globalFilesAdded) return files;
		else globalFilesAdded = true;
		if (files == null) files = new List<string>();

		// add all files from paths
		getFilesInPath(Application.dataPath + "/Editor/Reign/", files);
		getFilesInPath(Application.dataPath + "/Plugins/Reign/Resources/", files);
		getFilesInPath(Application.dataPath + "/Plugins/Reign/DemoScenes/", files);

		getFilesInPath(Application.dataPath + "/Plugins/Reign/Platforms/EditorRuntime/", files);
		getFilesInPath(Application.dataPath + "/Plugins/Reign/Managers/", files);
		getFilesInPath(Application.dataPath + "/Plugins/Reign/Math/", files);
		getFilesInPath(Application.dataPath + "/Plugins/Reign/Services/", files);
		getFilesInPath(Application.dataPath + "/Plugins/Reign/PluginAssets/", files);
		getFilesInPath(Application.dataPath + "/Plugins/Reign/Tools/", files);
		getFilesInPath(Application.dataPath + "/Plugins/Reign/Platforms/Shared/Code/", files);
		getFilesInPath(Application.dataPath + "/Plugins/Reign/Platforms/Shared/Interfaces/", files);
		getFilesInPath(Application.dataPath + "/Plugins/Reign/Platforms/Shared/ImageTools/", files);
		getFilesInPath(Application.dataPath + "/Plugins/Reign/Platforms/Shared/SharpZipLib/", files);

		// add specific files
		files.Add(Application.dataPath + "/Plugins/Reign/VersionInfo/ReignVersionCheck");
		files.Add(Application.dataPath + "/Plugins/Reign/ReadMe.txt");
		files.Add(Application.dataPath + "/gmcs.rsp");
		files.Add(Application.dataPath + "/smcs.rsp");

		return files;
	}

	private static void exportUnityPackages(List<string> files, string packageName, string customOutputPath)
	{
		string fileName;
		if (string.IsNullOrEmpty(customOutputPath))
		{
			fileName = EditorUtility.SaveFilePanel("Export Unitypackage", "", packageName, "unitypackage");
			if (string.IsNullOrEmpty(fileName)) return;
		}
		else
		{
			fileName = customOutputPath + packageName + ".unitypackage";
		}

		var fileNameArray = files.ToArray();
		for (int i = 0; i != fileNameArray.Length; ++i)
		{
			fileNameArray[i] = convertPathToPlatform("Assets" + fileNameArray[i].Substring(Application.dataPath.Length, fileNameArray[i].Length-Application.dataPath.Length));
			//Debug.Log("Export: " + fileNameArray[i]);
		}
		UnityEditor.AssetDatabase.ExportPackage(fileNameArray, fileName);
		Debug.Log("Exported package: " + fileName);
	}

	// WinRT
	private static List<string> getWindowsFiles(List<string> files)
	{
		files = getGlobalFiles(files);
		
		getFilesInPath(Application.dataPath + "/Plugins/Reign/Platforms/WinRT/", files);
		files.Add(Application.dataPath + "/Plugins/Reign/VersionInfo/ReignVersionCheck_WinRT");

		return files;
	}

	[MenuItem("Reign Dev/Create Unitypackage/Windows")]
	static void ExportWindowsUnityPackages()
	{
		globalFilesAdded = false;
		exportUnityPackages(getWindowsFiles(null), "Windows", null);
	}

	// BB10
	private static List<string> getBB10Files(List<string> files)
	{
		files = getGlobalFiles(files);

		getFilesInPath(Application.dataPath + "/Plugins/Reign/Platforms/BB10/", files);
		files.Add(Application.dataPath + "/Plugins/Reign/VersionInfo/ReignVersionCheck_BB10");

		return files;
	}

	[MenuItem("Reign Dev/Create Unitypackage/BB10")]
	static void ExportBB10UnityPackages()
	{
		globalFilesAdded = false;
		exportUnityPackages(getBB10Files(null), "BB10", null);
	}

	// iOS
	private static List<string> getIOSFiles(List<string> files)
	{
		files = getGlobalFiles(files);

		getFilesInPath(Application.dataPath + "/Plugins/iOS/", files);
		getFilesInPath(Application.dataPath + "/Plugins/Reign/Platforms/iOS/", files);
		files.Add(Application.dataPath + "/Plugins/Reign/VersionInfo/ReignVersionCheck_iOS");

		return files;
	}

	[MenuItem("Reign Dev/Create Unitypackage/iOS")]
	static void ExportIOSUnityPackages()
	{
		globalFilesAdded = false;
		exportUnityPackages(getIOSFiles(null), "iOS", null);
	}

	// Android
	private static List<string> getAndroidFiles(List<string> files)
	{
		files = getGlobalFiles(files);

		getFilesInPath(Application.dataPath + "/Plugins/Android/", files);
		getFilesInPath(Application.dataPath + "/Plugins/Reign/Platforms/Android/", files);
		files.Add(Application.dataPath + "/Plugins/Reign/VersionInfo/ReignVersionCheck_Android");

		return files;
	}

	[MenuItem("Reign Dev/Create Unitypackage/Android")]
	static void ExportAndroidUnityPackages()
	{
		globalFilesAdded = false;
		exportUnityPackages(getAndroidFiles(null), "Android", null);
	}

	// All
	private static List<string> getAllFiles(List<string> currentFiles)
	{
		var files = getGlobalFiles(currentFiles);
		getWindowsFiles(files);
		getBB10Files(files);
		getIOSFiles(files);
		getAndroidFiles(files);
		return files;
	}

	[MenuItem("Reign Dev/Create Unitypackage/Ultimate")]
	static void ExportUltimateUnityPackages()
	{
		globalFilesAdded = false;
		exportUnityPackages(getAllFiles(null), "Ultimate", null);
	}

	// All individual packages
	[MenuItem("Reign Dev/Create Unitypackage/All Individual Packages")]
	static void ExportAllUnityPackages()
	{
		string folder = EditorUtility.SaveFolderPanel("Export", "", "Data");
		if (string.IsNullOrEmpty(folder)) return;
		folder += "/";

		globalFilesAdded = false;
		exportUnityPackages(getWindowsFiles(null), "Windows", folder);

		globalFilesAdded = false;
		exportUnityPackages(getBB10Files(null), "BB10", folder);

		globalFilesAdded = false;
		exportUnityPackages(getIOSFiles(null), "iOS", folder);

		globalFilesAdded = false;
		exportUnityPackages(getAndroidFiles(null), "Android", folder);

		globalFilesAdded = false;
		exportUnityPackages(getAllFiles(null), "Ultimate", folder);
	}

	// Open Source Tools
	private static List<string> getOpenSourceFiles(List<string> files)
	{
		if (files == null) files = new List<string>();

		getFilesInPath(Application.dataPath + "/Plugins/Reign/Platforms/Shared/ImageTools/", files);
		getFilesInPath(Application.dataPath + "/Plugins/Reign/Platforms/Shared/SharpZipLib/", files);

		return files;
	}

	[MenuItem("Reign Dev/Create Unitypackage/Open Source Tools")]
	static void ExportOpenSourceUnityPackages()
	{
		string folder = EditorUtility.SaveFolderPanel("Export", "", "Data");
		if (string.IsNullOrEmpty(folder)) return;
		folder += "/";

		globalFilesAdded = false;
		exportUnityPackages(getOpenSourceFiles(null), "OpenSource", folder);
	}
}