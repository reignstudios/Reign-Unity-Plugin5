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
			var files = getAllFiles(null, false);
			foreach (var file in files)
			{
				if (Path.GetExtension(file) == ".DS_Store") continue;
				string value = file.Remove(0, Application.dataPath.Length).Replace('\\', '/');
				switch (value)
				{
					case "/Editor/Reign/Reign.EditorTools.dll":
					case "/Plugins/Reign/VersionInfo/ReignVersionCheck":
					case "/Plugins/Reign/VersionInfo/ReignVersionCheck_Windows":
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
		zipPath("src=" + rootPath + @"\Native\Reign.Android", "dst=" + Application.dataPath + "/Plugins/Android/Reign.Android.zip");

		AssetDatabase.Refresh();
		Debug.Log("Prepare Done!");
	}

	[MenuItem("Reign Dev/Add Version Number")]
	static void AddVersionNumber()
	{
		string path = Application.dataPath + "/Plugins/Reign/VersionInfo/";
		addVersion(convertPathToPlatform(path+"ReignVersionCheck"));
		addVersion(convertPathToPlatform(path+"ReignVersionCheck_Windows"));
		addVersion(convertPathToPlatform(path+"ReignVersionCheck_BB10"));
		addVersion(convertPathToPlatform(path+"ReignVersionCheck_iOS"));
		addVersion(convertPathToPlatform(path+"ReignVersionCheck_Android"));
	}

	[MenuItem("Reign Dev/Sub Version Number")]
	static void SubVersionNumber()
	{
		string path = Application.dataPath + "/Plugins/Reign/VersionInfo/";
		subVersion(convertPathToPlatform(path+"ReignVersionCheck"));
		subVersion(convertPathToPlatform(path+"ReignVersionCheck_Windows"));
		subVersion(convertPathToPlatform(path+"ReignVersionCheck_BB10"));
		subVersion(convertPathToPlatform(path+"ReignVersionCheck_iOS"));
		subVersion(convertPathToPlatform(path+"ReignVersionCheck_Android"));
	}

	private static void addVersion(string fileName)
	{
		int version;
		string key = readFileVersion(fileName, out version);
		writeFileVersion(fileName, version+1, key);
	}

	private static void subVersion(string fileName)
	{
		int version;
		string key = readFileVersion(fileName, out version);
		writeFileVersion(fileName, version-1, key);
	}

	private static string readFileVersion(string fileName, out int version)
	{
		using (var file = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None))
		using (var reader = new StreamReader(file))
		{
			version = int.Parse(reader.ReadLine());
			return reader.ReadLine();
		}
	}

	private static void writeFileVersion(string fileName, int version, string key)
	{
		using (var file = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
		using (var writer = new StreamWriter(file))
		{
			writer.WriteLine(version.ToString());
			writer.Write(key);
		}
	}

	private static string convertPathToPlatform(string path)
	{
		return path.Replace('\\', '/');
	}

	private static void getFilesInPath(string path, List<string> files) {getFilesInPath(path, files, null);}
	private static void getFilesInPath(string path, List<string> files, string ignoredFile)
	{
		// search sub directories
		foreach (var dir in Directory.GetDirectories(path))
		{
			if (Path.GetFileName(dir) != "PublisherOnly") getFilesInPath(dir, files, ignoredFile);
		}

		// add all files but .meta files
		foreach (var file in Directory.GetFiles(path))
		{
			if (Path.GetExtension(file) != ".meta" && Path.GetFileName(file) != ignoredFile) files.Add(file);
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

	private static void exportUnityPackages(List<string> files, bool disableUnityLegalConflicts, string packageName, string customOutputPath)
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

		getFilesInPath(Application.dataPath + "/Plugins/WP8/", files);
		getFilesInPath(Application.dataPath + "/Plugins/Reign/Platforms/WP8/", files);
		getFilesInPath(Application.dataPath + "/Plugins/Reign/Platforms/Shared/WinRT/", files);
		files.Add(Application.dataPath + "/Plugins/Reign/VersionInfo/ReignVersionCheck_Windows");

		return files;
	}

	[MenuItem("Reign Dev/Create Unitypackage/Windows")]
	static void ExportWindowsUnityPackages()
	{
		globalFilesAdded = false;
		exportUnityPackages(getWindowsFiles(null), false, "Windows", null);
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
		exportUnityPackages(getBB10Files(null), false, "BB10", null);
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
		exportUnityPackages(getIOSFiles(null), false, "iOS", null);
	}

	// Android
	private static List<string> getAndroidFiles(List<string> files, bool disableUnityLegalConflicts)
	{
		files = getGlobalFiles(files);

		getFilesInPath(Application.dataPath + "/Plugins/Android/", files, disableUnityLegalConflicts ? "amazon-ads-5.4.227.jar" : null);
		getFilesInPath(Application.dataPath + "/Plugins/Reign/Platforms/Android/", files);
		files.Add(Application.dataPath + "/Plugins/Reign/VersionInfo/ReignVersionCheck_Android");

		return files;
	}

	[MenuItem("Reign Dev/Create Unitypackage/Android")]
	static void ExportAndroidUnityPackages()
	{
		globalFilesAdded = false;
		exportUnityPackages(getAndroidFiles(null, false), false, "Android", null);
	}

	[MenuItem("Reign Dev/Create Unitypackage/Android-DisableUnityLegalConflicts")]
	static void ExportAndroidUnityPackages_DisableUnityLegalConflicts()
	{
		globalFilesAdded = false;
		exportUnityPackages(getAndroidFiles(null, true), true, "Android", null);
	}

	// All
	private static List<string> getAllFiles(List<string> currentFiles, bool disableUnityLegalConflicts)
	{
		var files = getGlobalFiles(currentFiles);
		getWindowsFiles(files);
		getBB10Files(files);
		getIOSFiles(files);
		getAndroidFiles(files, disableUnityLegalConflicts);
		return files;
	}

	[MenuItem("Reign Dev/Create Unitypackage/Ultimate")]
	static void ExportUltimateUnityPackages()
	{
		globalFilesAdded = false;
		exportUnityPackages(getAllFiles(null, false), false, "Ultimate", null);
	}

	[MenuItem("Reign Dev/Create Unitypackage/Ultimate-DisableUnityLegalConflicts")]
	static void ExportUltimateUnityPackages_DisableUnityLegalConflicts()
	{
		globalFilesAdded = false;
		exportUnityPackages(getAllFiles(null, true), true, "Ultimate", null);
	}

	// All individual packages
	[MenuItem("Reign Dev/Create Unitypackage/All Individual Packages")]
	static void ExportAllUnityPackages()
	{
		string folder = EditorUtility.SaveFolderPanel("Export", "", "Data");
		if (string.IsNullOrEmpty(folder)) return;
		folder += "/";

		globalFilesAdded = false;
		exportUnityPackages(getWindowsFiles(null), false, "Windows", folder);

		globalFilesAdded = false;
		exportUnityPackages(getBB10Files(null), false, "BB10", folder);

		globalFilesAdded = false;
		exportUnityPackages(getIOSFiles(null), false, "iOS", folder);

		globalFilesAdded = false;
		exportUnityPackages(getAndroidFiles(null, false), false, "Android", folder);

		globalFilesAdded = false;
		exportUnityPackages(getAllFiles(null, false), false, "Ultimate", folder);
	}

	[MenuItem("Reign Dev/Create Unitypackage/All Individual Packages - DisableUnityLegalConflicts")]
	static void ExportAllUnityPackages_DisableUnityLegalConflicts()
	{
		string folder = EditorUtility.SaveFolderPanel("Export", "", "Data");
		if (string.IsNullOrEmpty(folder)) return;
		folder += "/";

		globalFilesAdded = false;
		exportUnityPackages(getWindowsFiles(null), true, "Windows", folder);

		globalFilesAdded = false;
		exportUnityPackages(getBB10Files(null), true, "BB10", folder);

		globalFilesAdded = false;
		exportUnityPackages(getIOSFiles(null), true, "iOS", folder);

		globalFilesAdded = false;
		exportUnityPackages(getAndroidFiles(null, true), true, "Android", folder);

		globalFilesAdded = false;
		exportUnityPackages(getAllFiles(null, true), true, "Ultimate", folder);
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
		exportUnityPackages(getOpenSourceFiles(null), false, "OpenSource", folder);
	}
}