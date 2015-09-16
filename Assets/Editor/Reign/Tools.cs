// -------------------------------------------------------
//  Created by Andrew Witte.
// -------------------------------------------------------

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using Reign;

using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.IO;
using System.Text;
using System;
using System.Collections.Generic;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace Reign.EditorTools
{
	public static class Tools
	{
		#region GeneralTools
		[MenuItem("Edit/Reign/Tools/Print New Guid")]
		static void PrintNewGuid()
		{
			Debug.Log(Guid.NewGuid());
		}

		[MenuItem("Edit/Reign/Tools/Merge Folders")]
		static void MergeFolders()
		{
			string src = EditorUtility.OpenFolderPanel("Src Folder", "", "");
			if (string.IsNullOrEmpty(src)) return;
			string dst = EditorUtility.OpenFolderPanel("Dst Folder", "", "");
			if (string.IsNullOrEmpty(dst)) return;

			Debug.Log("Src Folder: " + src);
			Debug.Log("Dst Folder: " + dst);
			var files = new List<string>();
			gatherFilePaths(src, files, true);
			foreach (var file in files)
			{
				string newDst = dst + file.Substring(src.Length);
				Directory.CreateDirectory(Path.GetDirectoryName(newDst));
				File.Copy(file, newDst, true);
			}

			AssetDatabase.Refresh();
			Debug.Log("Merge Folders Done!");
		}

		static void gatherFilePaths(string path, List<string> files, bool recursive)
		{
			// add files in path
			var dir = new DirectoryInfo(path);
			foreach (var file in dir.GetFiles())
			{
				if ((file.Attributes & FileAttributes.Hidden) == 0 && (file.Attributes & FileAttributes.Directory) == 0) files.Add(file.FullName);
			}

			// add sub paths
			if (recursive)
			foreach (var subPath in Directory.GetDirectories(path))
			{
				gatherFilePaths(subPath, files, recursive);
			}
		}

		[MenuItem("Edit/Reign/Tools/Clear All PlayerPrefs")]
		static void InitClearAll()
		{
			PlayerPrefs.DeleteAll();
			Debug.Log("PlayerPrefs Cleared!");
		}
	
		[MenuItem("Edit/Reign/Tools/Reset Editor InApps Prefs (While game is running)")]
		static void InitClearInApps()
		{
			if (InAppPurchaseManager.InAppAPIs == null)
			{
				Debug.LogError("The app must be running with the IAP system initialized!");
				return;
			}
	
			foreach (var api in InAppPurchaseManager.InAppAPIs)
			{
				api.ClearPlayerPrefData();
			}

			Debug.Log("PlayerPrefs for IAP Only Cleared!");
		}

		[MenuItem("Edit/Reign/Clean (For troubleshooting only!)")]
		static void Clean()
		{
			if (!EditorUtility.DisplayDialog("Warning!", "This will remove all Reign plugin files.", "OK", "Cancel")) return;

			using (var stream = new FileStream(Application.dataPath+"/Editor/Reign/CleanSettings", FileMode.Open, FileAccess.Read, FileShare.None))
			using (var reader = new StreamReader(stream))
			{
				string file = reader.ReadLine();
				while (!string.IsNullOrEmpty(file))
				{
					file = Application.dataPath + file;
					try
					{
						if (File.Exists(file)) File.Delete(file);
					}
					catch
					{
						Debug.LogError("Failed to delete file: " + file);
					}

					file = reader.ReadLine();
				}
			}

			AssetDatabase.Refresh();
			Debug.Log("Clean Done!");
		}
		#endregion

		#region PostBuildTools
		static bool postProjectCompilerDirectiveExists(string value, XElement element)
		{
			foreach (var name in element.Value.Split(';', ' '))
			{
				if (name == value) return true;
			}

			return false;
		}

		static void addPostProjectCompilerDirectives(XDocument doc)
		{
			foreach (var element in doc.Root.Elements())
			{
				if (element.Name.LocalName != "PropertyGroup") continue;
				foreach (var subElement in element.Elements())
				{
					if (subElement.Name.LocalName == "DefineConstants")
					{
						// make sure we need to add compiler directive
						if (!postProjectCompilerDirectiveExists("REIGN_POSTBUILD", subElement)) subElement.Value += ";REIGN_POSTBUILD";
						#if WINRT_DISABLE_MS_ADS
						if (!postProjectCompilerDirectiveExists("WINRT_DISABLE_MS_ADS", subElement)) subElement.Value += ";WINRT_DISABLE_MS_ADS";
						#endif
						#if WINRT_DISABLE_GOOGLE_ADS
						if (!postProjectCompilerDirectiveExists("WINRT_DISABLE_GOOGLE_ADS", subElement)) subElement.Value += ";WINRT_DISABLE_GOOGLE_ADS";
						#endif
						#if WINRT_DISABLE_ADDUPLEX_ADS
						if (!postProjectCompilerDirectiveExists("WINRT_DISABLE_ADDUPLEX_ADS", subElement)) subElement.Value += ";WINRT_DISABLE_ADDUPLEX_ADS";
						#endif
						#if WINRT_DISABLE_MS_IAP
						if (!postProjectCompilerDirectiveExists("WINRT_DISABLE_MS_IAP", subElement)) subElement.Value += ";WINRT_DISABLE_MS_IAP";
						#endif
					}
				}
			}
		}

		static void addPostProjectReferences(XDocument doc, string pathToBuiltProject, string extraPath, string productName, string extraRefValue)
		{
			XElement sourceElementRoot = null;
			foreach (var element in doc.Root.Elements())
			{
				if (element.Name.LocalName != "ItemGroup") continue;
				foreach (var subElement in element.Elements())
				{
					if (subElement.Name.LocalName == "Compile")
					{
						sourceElementRoot = element;
						break;
					}
				}

				if (sourceElementRoot != null) break;
			}

			if (sourceElementRoot != null)
			{
				var csSources = new string[]
				{
					"Shared/WinRT/EmailPlugin.cs",
					"Shared/WinRT/MarketingPlugin.cs",
					"Shared/WinRT/MessageBoxPlugin.cs",
					#if !WINRT_DISABLE_MS_ADS
					"Shared/WinRT/MicrosoftAdvertising_AdPlugin.cs",
					#endif
					#if !WINRT_DISABLE_MS_IAP
					"Shared/WinRT/MicrosoftStore_InAppPurchasePlugin.cs",
					#endif
					"Shared/WinRT/StreamPlugin.cs",
					"Shared/WinRT/SocialPlugin.cs",
					"Shared/WinRT/WinRTPlugin.cs",

					#if UNITY_WP8
					#if !WINRT_DISABLE_GOOGLE_ADS
					"WP8/AdMob_AdPlugin.cs",
					"WP8/AdMob_InterstitialAdPlugin.cs",
					#endif

					#if !WINRT_DISABLE_MS_IAP
					"WP8/CurrentAppSimulator/CurrentApp.cs",
					"WP8/CurrentAppSimulator/LicenseInformation.cs",
					"WP8/CurrentAppSimulator/ListingInformation.cs",
					"WP8/CurrentAppSimulator/MockIAP.cs",
					"WP8/CurrentAppSimulator/MockReceiptState.cs",
					"WP8/CurrentAppSimulator/MockReceiptStore.cs",
					"WP8/CurrentAppSimulator/ProductLicense.cs",
					"WP8/CurrentAppSimulator/ProductListing.cs",
					#endif
					#elif !WINRT_DISABLE_ADDUPLEX_ADS
					"Shared/WinRT/AdDuplex_AdPlugin.cs",
					"Shared/WinRT/AdDuplex_InterstitialAdPlugin.cs",
					#endif
				};

				foreach (var source in csSources)
				{
					// copy cs file
					string sourcePath = string.Format("{0}/{1}/{2}", Application.dataPath, "Plugins/Reign/Platforms", source);
					string sourceFileName = Path.GetFileName(source);
					File.Copy(sourcePath, string.Format("{0}/{1}{2}/{3}", pathToBuiltProject, productName, extraPath, sourceFileName), true);

					// make sure we need to reference the file
					bool needToRefFile = true;
					foreach (var element in sourceElementRoot.Elements())
					{
						if (element.Name.LocalName == "Compile")
						{
							foreach (var a in element.Attributes())
							{
								if (a.Name.LocalName == "Include" && a.Value == (extraRefValue + sourceFileName))
								{
									needToRefFile = false;
									break;
								}
							}
						}

						if (!needToRefFile) break;
					}

					// add reference to cs proj
					if (needToRefFile)
					{
						var name = XName.Get("Compile", doc.Root.GetDefaultNamespace().NamespaceName);
						var newSource = new XElement(name);
						newSource.SetAttributeValue(XName.Get("Include"), extraRefValue + sourceFileName);
						sourceElementRoot.Add(newSource);
					}
				}
			}
			else
			{
				Debug.LogError("Reign Post Build Error: Failed to find CS source element in proj!");
			}
		}

		[PostProcessBuild]
		static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
		{
			#if DISABLE_REIGN
			return;
			#endif

			if (target == BuildTarget.WSAPlayer || target == BuildTarget.WP8Player)
			{
				var productName = PlayerSettings.productName;
				
				if (EditorUserBuildSettings.wsaSDK == WSASDK.UniversalSDK81 && EditorUserBuildSettings.activeBuildTarget != BuildTarget.WP8Player)
				{
					var projPath = string.Format("{0}/{1}/{1}.Shared/{1}.Shared.projItems", pathToBuiltProject, productName);
					Debug.Log("Modifying Proj: " + projPath);
					var doc = XDocument.Load(projPath);
					addPostProjectReferences(doc, pathToBuiltProject, string.Format("/{0}.Shared", productName), productName, "$(MSBuildThisFileDirectory)");
					doc.Save(projPath);

					projPath = string.Format("{0}/{1}/{1}.Windows/{1}.Windows.csproj", pathToBuiltProject, productName);
					Debug.Log("Modifying Proj: " + projPath);
					doc = XDocument.Load(projPath);
					addPostProjectCompilerDirectives(doc);
					doc.Save(projPath);

					projPath = string.Format("{0}/{1}/{1}.WindowsPhone/{1}.WindowsPhone.csproj", pathToBuiltProject, productName);
					Debug.Log("Modifying Proj: " + projPath);
					doc = XDocument.Load(projPath);
					addPostProjectCompilerDirectives(doc);
					doc.Save(projPath);
				}
				else
				{
					var projPath = string.Format("{0}/{1}/{1}.csproj", pathToBuiltProject, productName);
					Debug.Log("Modifying Proj: " + projPath);

					var doc = XDocument.Load(projPath);
					addPostProjectCompilerDirectives(doc);
					addPostProjectReferences(doc, pathToBuiltProject, "", productName, "");
					doc.Save(projPath);
				}
			}
			#if UNITY_IOS
			else if (target == BuildTarget.iOS)
			{
				string projPath = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";

				var proj = new PBXProject();
				proj.ReadFromString(File.ReadAllText (projPath));

				string targetID = proj.TargetGuidByName ("Unity-iPhone");

				// set custom link flags
				proj.AddBuildProperty (targetID, "OTHER_LDFLAGS", "-all_load");
				proj.AddBuildProperty (targetID, "OTHER_LDFLAGS", "-ObjC");

				// add frameworks
				proj.AddFrameworkToProject(targetID, "MessageUI.framework", true);
				#if !IOS_DISABLE_APPLE_IAP
				proj.AddFrameworkToProject(targetID, "StoreKit.framework", true);
				proj.AddFrameworkToProject(targetID, "Security.framework", true);
				#endif
				#if !IOS_DISABLE_APPLE_ADS
				proj.AddFrameworkToProject(targetID, "iAd.framework", true);
				#endif
				#if !IOS_DISABLE_APPLE_SCORES
				proj.AddFrameworkToProject(targetID, "GameKit.framework", true);
				#endif
				#if !IOS_DISABLE_GOOGLE_ADS
				proj.AddFrameworkToProject(targetID, "GoogleMobileAds.framework", false);
				proj.AddFrameworkToProject(targetID, "AdSupport.framework", true);
				proj.AddFrameworkToProject(targetID, "CoreTelephony.framework", true);
				proj.AddFrameworkToProject(targetID, "EventKit.framework", true);
				proj.AddFrameworkToProject(targetID, "EventKitUI.framework", true);
				#endif

				// change GoogleMobileAds to use reletive path
				string projData = proj.WriteToString();
				projData = projData.Replace
				(
					@"/* GoogleMobileAds.framework */ = {isa = PBXFileReference; lastKnownFileType = wrapper.framework; name = GoogleMobileAds.framework; path = System/Library/Frameworks/GoogleMobileAds.framework; sourceTree = SDKROOT; };",
					@"/* GoogleMobileAds.framework */ = {isa = PBXFileReference; lastKnownFileType = wrapper.framework; name = GoogleMobileAds.framework; path = Frameworks/GoogleMobileAds.framework; sourceTree = ""<group>""; };"
					//@"/* GoogleMobileAds.framework */ = {isa = PBXFileReference; lastKnownFileType = wrapper.framework; name = GoogleMobileAds.framework; path = """ + pathToBuiltProject+"/Frameworks/GoogleMobileAds.framework" + @"""; sourceTree = ""<absolute>""; };"
				);

				// change framework search path to include local framework directory
				projData = projData.Replace
				(
					@"FRAMEWORK_SEARCH_PATHS = ""$(inherited)"";",
					@"FRAMEWORK_SEARCH_PATHS = (""$(inherited)"", ""$(PROJECT_DIR)/Frameworks"",);"
				);

				// save proj data
				File.WriteAllText(projPath, projData);

				// create Frameworks folder if one doesn't exists
				if (!Directory.Exists(pathToBuiltProject+"/Frameworks/")) Directory.CreateDirectory(pathToBuiltProject+"/Frameworks/");

				// extract GoogleMobileAds.framework.zip to xcode framework path
				if (!Directory.Exists(pathToBuiltProject+"/Frameworks/GoogleMobileAds.framework/"))
				{
					var startInfo = new System.Diagnostics.ProcessStartInfo();
					startInfo.Arguments = @"""" + Application.dataPath+"/Plugins/IOS/GoogleMobileAds.framework.zip" + @""" -d """ + pathToBuiltProject+@"/Frameworks/""";
					startInfo.FileName = "unzip";
					startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
					startInfo.CreateNoWindow = true;
					using (var process = System.Diagnostics.Process.Start(startInfo))
					{
						process.WaitForExit();
						int exitCode = process.ExitCode;
						if (exitCode != 0) Debug.LogError("Failed to unzip GoogleMobileAds.framework.zip with ErrorCode: " + exitCode);
					}
				}
			}
			#endif
			else if (target == BuildTarget.Android)
			{
				if (!EditorUserBuildSettings.exportAsGoogleAndroidProject)
				{
					Debug.LogWarning("Reign post build step canceled.  Must export as GoogleAndroidProject to use the Reign plugin.");
					return;
				}

				Debug.Log("Processing Android Proj: " + pathToBuiltProject + '/' + PlayerSettings.productName);
				androidPostBuild(target, pathToBuiltProject + '/' + PlayerSettings.productName);
			}
    	}
		#endregion

		#if REIGN_TEST
		[MenuItem("ReignTest/AndroidBuildTest")]
		static void testAndroidBUILD()
		{
			#if UNITY_EDITOR_OSX
			androidPostBuild(BuildTarget.Android, "/Users/andrew/Dev/Reign/Reign-Unity-Plugin5-Builds/Android/Reign Unity Plugin");
			#else
			androidPostBuild(BuildTarget.Android, @"C:\Users\zezba\Dev\Reign\Reign-Unity-Plugin5-Builds\Android\Reign Unity Plugin");
			#endif
		}
		#endif

		private static void androidPostBuild(BuildTarget target, string pathToBuiltProject)
		{
			pathToBuiltProject = pathToBuiltProject.Replace(@"\", "/");
			string rootPath = Path.GetDirectoryName(pathToBuiltProject);
			#if GOOGLEPLAY
			string postBuildName = "GooglePlay";
			#elif AMAZON
			string postBuildName = "Amazon";
			#elif SAMSUNG
			string postBuildName = "Samsung";
			#else
			string postBuildName = "";
			#endif
			string mainPath = pathToBuiltProject.Substring(rootPath.Length).Replace(' ', '_').Replace("/", "").Replace(@"\", "") + "_PostBuild" + postBuildName;
			string fullPath = rootPath + '/' + mainPath;
			bool mergeMode = true;
			if (!Directory.Exists(fullPath))
			{
				mergeMode = false;
				Directory.CreateDirectory(fullPath);
			}
			
			// create paths
			Directory.CreateDirectory(fullPath + "/app/libs");
			Directory.CreateDirectory(fullPath + "/app/src/main/assets");
			Directory.CreateDirectory(fullPath + "/app/src/main/aidl");
			Directory.CreateDirectory(fullPath + "/app/src/main/java");
			Directory.CreateDirectory(fullPath + "/app/src/main/jniLibs");
			Directory.CreateDirectory(fullPath + "/app/src/main/res");
			Directory.CreateDirectory(fullPath + "/gradle");
			Directory.CreateDirectory(fullPath + "/.idea/copyright");
			
			// copy app files
			if (!mergeMode) File.Copy(pathToBuiltProject+"/AndroidManifest.xml", fullPath+"/app/src/main/AndroidManifest.xml", true);
			mergeFolders(pathToBuiltProject+"/assets", fullPath+"/app/src/main/assets", true, false);
			mergeFolders(pathToBuiltProject+"/res", fullPath+"/app/src/main/res", true, false);
			if (Directory.Exists(pathToBuiltProject+"/aidl")) mergeFolders(pathToBuiltProject+"/aidl", fullPath+"/app/src/main/aidl", true, false);
			if (!mergeMode) mergeFolders(pathToBuiltProject+"/src", fullPath+"/app/src/main/java", true, false);
			mergeFolders(pathToBuiltProject+"/libs", fullPath+"/app/libs", false, false);
			mergeFolders(pathToBuiltProject+"/libs/armeabi-v7a", fullPath+"/app/src/main/jniLibs/armeabi-v7a", false, false);
			mergeFolders(pathToBuiltProject+"/libs/x86", fullPath+"/app/src/main/jniLibs/x86", false, false);

			// get lib names
			var libs = new List<string>();
			var dir = new DirectoryInfo(pathToBuiltProject + "/libs");
			foreach (var file in dir.GetFiles())
			{
				if ((file.Attributes & FileAttributes.Hidden) == 0 && (file.Attributes & FileAttributes.Directory) == 0 && file.Extension == ".jar")
				{
					libs.Add(file.Name);
				}
			}

			// get android sdk target version
			string androidSDKVersion;
			using (var stream = new FileStream(pathToBuiltProject + "/project.properties", FileMode.Open, FileAccess.Read, FileShare.None))
			using (var readerProp = new StreamReader(stream))
			{
				androidSDKVersion = readerProp.ReadLine().Replace("target=android-", "");
			}

			// generated app files >>>
			// <<< read build.gradle file
			if (!mergeMode)
			using (var streamSrc = new FileStream(Application.dataPath + "/Plugins/Android/ReignPostBuildResources/app/build.gradle", FileMode.Open, FileAccess.Read, FileShare.None))
			using (var reader = new StreamReader(streamSrc))
			{
				string value = reader.ReadToEnd();
				value = value.Replace(@"applicationId ""com.Company.AppName""", string.Format(@"applicationId ""{0}""", PlayerSettings.bundleIdentifier));
				value = value.Replace("compileSdkVersion ??", "compileSdkVersion " + androidSDKVersion);
				value = value.Replace("targetSdkVersion ??", "targetSdkVersion " + androidSDKVersion);
				value = value.Replace("minSdkVersion ??", "minSdkVersion " + (int)PlayerSettings.Android.minSdkVersion);
				
				using (var streamDst = new FileStream(fullPath + "/app/build.gradle", FileMode.Create, FileAccess.Write, FileShare.None))
				using (var writer = new StreamWriter(streamDst))
				{
					// write build.gradle
					writer.WriteLine(value);
					writer.WriteLine(Environment.NewLine + "dependencies {");
					foreach (var lib in libs) writer.WriteLine(string.Format("    compile files('libs/{0}')", lib));
					#if GOOGLEPLAY && !ANDROID_DISABLE_GOOGLEPLAY
					writer.WriteLine("    compile files('libs/google-play-services.jar')");
					#elif AMAZON && !ANDROID_DISABLE_AMAZON
					#if !ANDROID_DISABLE_AMAZON_ADS
					writer.WriteLine("    compile files('libs/amazon-ads-5.6.20.jar')");
					#endif
					#if !ANDROID_DISABLE_AMAZON_SCORES
					writer.WriteLine("    compile files('libs/AmazonInsights-android-sdk-2.1.26.jar')");
					writer.WriteLine("    compile files('libs/gamecirclesdk.jar')");
					writer.WriteLine("    compile files('libs/login-with-amazon-sdk.jar')");
					#endif
					#if !ANDROID_DISABLE_AMAZON_IAP
					writer.WriteLine("    compile files('libs/in-app-purchasing-2.0.61.jar')");
					#endif
					#endif
					writer.WriteLine("}");
				}
			}

			// <<< read app.iml file
			if (!mergeMode)
			using (var streamSrc = new FileStream(Application.dataPath + "/Plugins/Android/ReignPostBuildResources/app/app.iml", FileMode.Open, FileAccess.Read, FileShare.None))
			using (var reader = new StreamReader(streamSrc))
			{
				string value = reader.ReadToEnd();
				value = value.Replace("REIGN_OUT_FOLDER", mainPath);
				value = value.Replace(@"<orderEntry type=""jdk"" jdkName=""Android API ?? Platform"" jdkType=""Android SDK"" />", string.Format(@"<orderEntry type=""jdk"" jdkName=""Android API {0} Platform"" jdkType=""Android SDK"" />", androidSDKVersion));

				using (var streamDst = new FileStream(fullPath + "/app/app.iml", FileMode.Create, FileAccess.Write, FileShare.None))
				using (var writer = new StreamWriter(streamDst))
				{
					string libValues = @"<orderEntry type=""sourceFolder"" forTests=""false"" />";
					foreach (var lib in libs)
					{
						libValues += Environment.NewLine + string.Format(@"    <orderEntry type=""library"" exported="""" name=""{0}"" level=""project"" />", lib.Substring(0, lib.Length-4));
					}

					// write app.iml
					writer.WriteLine(value.Replace(@"<orderEntry type=""sourceFolder"" forTests=""false"" />", libValues));
				}
			}

			// copy proj files
			mergeFolders(Application.dataPath+"/Plugins/Android/ReignPostBuildResources/gradle", fullPath+"/gradle", true, true);
			File.Copy(Application.dataPath+"/Plugins/Android/ReignPostBuildResources/idea/copyright/profiles_settings.xml", fullPath+"/.idea/copyright/profiles_settings.xml", true);
			File.Copy(Application.dataPath+"/Plugins/Android/ReignPostBuildResources/idea/compiler.xml", fullPath+"/.idea/compiler.xml", true);
			File.Copy(Application.dataPath+"/Plugins/Android/ReignPostBuildResources/idea/encodings.xml", fullPath+"/.idea/encodings.xml", true);
			if (!mergeMode) File.Copy(Application.dataPath+"/Plugins/Android/ReignPostBuildResources/idea/gradle.xml", fullPath+"/.idea/gradle.xml", true);
			if (!mergeMode) File.Copy(Application.dataPath+"/Plugins/Android/ReignPostBuildResources/idea/runConfigurations.xml", fullPath+"/.idea/runConfigurations.xml", true);
			File.Copy(Application.dataPath+"/Plugins/Android/ReignPostBuildResources/idea/vcs.xml", fullPath+"/.idea/vcs.xml", true);
			if (!mergeMode) File.Copy(Application.dataPath+"/Plugins/Android/ReignPostBuildResources/build.gradle", fullPath+"/build.gradle", true);
			if (!mergeMode) File.Copy(Application.dataPath+"/Plugins/Android/ReignPostBuildResources/settings.gradle", fullPath+"/settings.gradle", true);
			File.Copy(Application.dataPath+"/Plugins/Android/ReignPostBuildResources/gradlew", fullPath+"/gradlew", true);
			File.Copy(Application.dataPath+"/Plugins/Android/ReignPostBuildResources/gradlew.bat", fullPath+"/gradlew.bat", true);

			// generated proj files >>>
			// <<< read settings.iml file
			if (!mergeMode)
			using (var streamSrc = new FileStream(Application.dataPath + "/Plugins/Android/ReignPostBuildResources/settings.iml", FileMode.Open, FileAccess.Read, FileShare.None))
			using (var reader = new StreamReader(streamSrc))
			{
				string value = reader.ReadToEnd();
				value = value.Replace("REIGN_OUT_FOLDER", mainPath);

				using (var streamDst = new FileStream(fullPath + string.Format("/{0}.iml", mainPath), FileMode.Create, FileAccess.Write, FileShare.None))
				using (var writer = new StreamWriter(streamDst))
				{
					// write settings.iml
					writer.WriteLine(value);
				}
			}

			// <<< read modules.xml file
			if (!mergeMode)
			using (var streamSrc = new FileStream(Application.dataPath + "/Plugins/Android/ReignPostBuildResources/idea/modules.xml", FileMode.Open, FileAccess.Read, FileShare.None))
			using (var reader = new StreamReader(streamSrc))
			{
				string value = reader.ReadToEnd();
				value = value.Replace("settings.iml", mainPath+".iml");

				using (var streamDst = new FileStream(fullPath + "/.idea/modules.xml", FileMode.Create, FileAccess.Write, FileShare.None))
				using (var writer = new StreamWriter(streamDst))
				{
					// write modules.xml
					writer.WriteLine(value);
				}
			}

			// <<< read misc.xml file
			if (!mergeMode)
			using (var streamSrc = new FileStream(Application.dataPath + "/Plugins/Android/ReignPostBuildResources/idea/misc.xml", FileMode.Open, FileAccess.Read, FileShare.None))
			using (var reader = new StreamReader(streamSrc))
			{
				string value = reader.ReadToEnd();
				value = value.Replace("Android API ?? Platform", string.Format("Android API {0} Platform", androidSDKVersion));

				using (var streamDst = new FileStream(fullPath + "/.idea/misc.xml", FileMode.Create, FileAccess.Write, FileShare.None))
				using (var writer = new StreamWriter(streamDst))
				{
					// write misc.xml
					writer.WriteLine(value);
				}
			}
			
			// merge native java/res/libs files
			mergeFolders(Application.dataPath+"/Plugins/Android/ReignPostBuildResources/java/com/reignstudios/reignnative", fullPath+"/app/src/main/java/com/reignstudios/reignnative", true, true);
			#if GOOGLEPLAY && !ANDROID_DISABLE_GOOGLEPLAY
			File.Copy(Application.dataPath+"/Plugins/Android/ReignPostBuildResources/libs/GooglePlay/google-play-services.jar.file", fullPath+"/app/libs/google-play-services.jar", true);
			mergeFolders(Application.dataPath+"/Plugins/Android/ReignPostBuildResources/aidl/GooglePlay", fullPath+"/app/src/main/aidl", true, true);
			mergeFolders(Application.dataPath+"/Plugins/Android/ReignPostBuildResources/java/com/example", fullPath+"/app/src/main/java/com/example", true, true);
			mergeFolders(Application.dataPath+"/Plugins/Android/ReignPostBuildResources/java/com/reignstudios/reignnativegoogleplay", fullPath+"/app/src/main/java/com/reignstudios/reignnativegoogleplay", true, true);
			mergeFolders(Application.dataPath+"/Plugins/Android/ReignPostBuildResources/res/GooglePlay", fullPath+"/app/src/main/res", true, true);
			#elif AMAZON && !ANDROID_DISABLE_AMAZON
			var ignoredJavaFiles = new List<string>();
			#if !ANDROID_DISABLE_AMAZON_ADS
			File.Copy(Application.dataPath+"/Plugins/Android/ReignPostBuildResources/libs/Amazon/amazon-ads-5.6.20.jar.file", fullPath+"/app/libs/amazon-ads-5.6.20.jar", true);
			#else
			ignoredJavaFiles.Add("Amazon_AdsNative.java");
			ignoredJavaFiles.Add("Amazon_InterstitialAdNative.java");
			#endif
			#if !ANDROID_DISABLE_AMAZON_SCORES
			File.Copy(Application.dataPath+"/Plugins/Android/ReignPostBuildResources/libs/Amazon/AmazonInsights-android-sdk-2.1.26.jar.file", fullPath+"/app/libs/AmazonInsights-android-sdk-2.1.26.jar", true);
			File.Copy(Application.dataPath+"/Plugins/Android/ReignPostBuildResources/libs/Amazon/gamecirclesdk.jar.file", fullPath+"/app/libs/gamecirclesdk.jar", true);
			File.Copy(Application.dataPath+"/Plugins/Android/ReignPostBuildResources/libs/Amazon/login-with-amazon-sdk.jar.file", fullPath+"/app/libs/login-with-amazon-sdk.jar", true);
			File.Copy(Application.dataPath+"/Plugins/Android/ReignPostBuildResources/libs/Amazon/jniLibs/armeabi-v7a/libAmazonGamesJni.so.file", fullPath+"/app/src/main/jniLibs/armeabi-v7a/libAmazonGamesJni.so", true);
			#else
			ignoredJavaFiles.Add("Amazon_GameCircle_LeaderboardsAchievements.java");
			#endif
			#if !ANDROID_DISABLE_AMAZON_IAP
			File.Copy(Application.dataPath+"/Plugins/Android/ReignPostBuildResources/libs/Amazon/in-app-purchasing-2.0.61.jar.file", fullPath+"/app/libs/in-app-purchasing-2.0.61.jar", true);
			#else
			ignoredJavaFiles.Add("Amazon_InAppPurchaseNative.java");
			#endif
			mergeFolders(Application.dataPath+"/Plugins/Android/ReignPostBuildResources/java/com/reignstudios/reignnativeamazon", fullPath+"/app/src/main/java/com/reignstudios/reignnativeamazon", true, true, ignoredJavaFiles.ToArray());
			#if !ANDROID_DISABLE_AMAZON_SCORES
			mergeFolders(Application.dataPath+"/Plugins/Android/ReignPostBuildResources/res/Amazon", fullPath+"/app/src/main/res", true, true);
			#endif
			#elif SAMSUNG && !ANDROID_DISABLE_SAMSUNG
			var ignoredJavaFiles = new List<string>();
			#if !ANDROID_DISABLE_SAMSUNG_IAP
			mergeFolders(Application.dataPath+"/Plugins/Android/ReignPostBuildResources/aidl/Samsung", fullPath+"/app/src/main/aidl", true, true);
			mergeFolders(Application.dataPath+"/Plugins/Android/ReignPostBuildResources/java/com/samsung", fullPath+"/app/src/main/java/com/samsung", true, true);
			mergeFolders(Application.dataPath+"/Plugins/Android/ReignPostBuildResources/res/Samsung", fullPath+"/app/src/main/res", true, true);
			replaceTextInJavaFiles(fullPath+"/app/src/main/java/com/samsung", "import com.samsung.android.sdk.iap.lib.R;", string.Format("import {0}.R;", PlayerSettings.bundleIdentifier));
			using (var streamSrc = new FileStream(Application.dataPath + "/Plugins/Android/ReignPostBuildResources/res/Samsung/values/strings.xml", FileMode.Open, FileAccess.Read, FileShare.None))
			using (var reader = new StreamReader(streamSrc))
			{
				string value = reader.ReadToEnd();
				value = value.Replace("PRODUCT_NAME", PlayerSettings.productName);

				using (var streamDst = new FileStream(fullPath + "/app/src/main/res/values/strings.xml", FileMode.Create, FileAccess.Write, FileShare.None))
				using (var writer = new StreamWriter(streamDst))
				{
					// write misc.xml
					writer.WriteLine(value);
				}
			}
			#else
			ignoredJavaFiles.Add("Samsung_InAppPurchaseNative.java");
			#endif
			mergeFolders(Application.dataPath+"/Plugins/Android/ReignPostBuildResources/java/com/reignstudios/reignnativesamsung", fullPath+"/app/src/main/java/com/reignstudios/reignnativesamsung", true, true, ignoredJavaFiles.ToArray());
			#endif

			Debug.Log("Reign Android Studio output: " + fullPath);
		}

		private static void replaceTextInJavaFiles(string path, string oldValue, string newValue)
		{
			var files = new List<string>();
			gatherFilePaths(path, files, true);
			foreach (var file in files)
			{
				if (Path.GetExtension(file) != ".java") continue;

				string value = null;
				using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read))
				using (var reader = new StreamReader(stream))
				{
					value = reader.ReadToEnd();
				}
				
				value = value.Replace(oldValue, newValue);
				using (var stream = new FileStream(file, FileMode.Create, FileAccess.Write))
				using (var writer = new StreamWriter(stream))
				{
					writer.Write(value);
				}
			}
		}

		private static void mergeFolders(string src, string dst, bool recursive, bool ignoreMetaExt)
		{
			mergeFolders(src, dst, recursive, ignoreMetaExt, null);
		}

		private static void mergeFolders(string src, string dst, bool recursive, bool ignoreMetaExt, string[] ignoredFiles)
		{
			var files = new List<string>();
			gatherFilePaths(src, files, recursive);
			foreach (var file in files)
			{
				if (ignoreMetaExt && Path.GetExtension(file) == ".meta") continue;

				if (ignoredFiles != null)
				{
					string name = Path.GetFileName(file);
					bool skip = false;
					foreach (var ignore in ignoredFiles)
					{
						if (ignore == name)
						{
							skip = true;
							break;
						}
					}

					if (skip) continue;
				}

				string newDst = dst + file.Substring(src.Length);
				Directory.CreateDirectory(Path.GetDirectoryName(newDst));
				File.Copy(file, newDst, true);
			}
		}

		#region PlatformTools
		internal static void applyCompilerDirectives(bool append, params string[] directives)
		{
			var platform = EditorUserBuildSettings.selectedBuildTargetGroup;
			string valueBlock = PlayerSettings.GetScriptingDefineSymbolsForGroup(platform);
			string newValue = "";
			if (string.IsNullOrEmpty(valueBlock))
			{
				foreach (var directive in directives)
				{
					newValue += directive;
					if (directive != directives[directives.Length-1]) newValue += ';';
				}

				PlayerSettings.SetScriptingDefineSymbolsForGroup(platform, newValue);
			}
			else
			{
				var values = valueBlock.Split(';', ' ');
				if (append)
				{
					foreach (var value in values)
					{
						newValue += value + ';';
					}
				}

				foreach (var directive in directives)
				{
					bool exists = false;
					foreach (var value in values)
					{
						if (value == directive)
						{
							exists = true;
							break;
						}
					}

					if (!exists)
					{
						newValue += directive;
						if (directive != directives[directives.Length-1]) newValue += ';';
					}
				}
				
				PlayerSettings.SetScriptingDefineSymbolsForGroup(platform, newValue);
			}

			Debug.Log("Compiler Directives set to: " + newValue);
		}

		internal static void removeCompilerDirectives(params string[] directives)
		{
			var platform = EditorUserBuildSettings.selectedBuildTargetGroup;
			string valueBlock = PlayerSettings.GetScriptingDefineSymbolsForGroup(platform);
			string newValue = "";
			if (!string.IsNullOrEmpty(valueBlock))
			{
				var values = valueBlock.Split(';', ' ');
				foreach (var value in values)
				{
					bool exists = false;
					foreach (var directive in directives)
					{
						if (value == directive)
						{
							exists = true;
							break;
						}
					}

					if (!exists) newValue += value + ';';
				}
				
				if (newValue.Length != 0 && newValue[newValue.Length-1] == ';') newValue = newValue.Substring(0, newValue.Length-1);
				PlayerSettings.SetScriptingDefineSymbolsForGroup(platform, newValue);
			}

			Debug.Log("Compiler Directives set to: " + newValue);
		}

		private static void clearCompilerDirectives()
		{
			var platform = EditorUserBuildSettings.selectedBuildTargetGroup;
			PlayerSettings.SetScriptingDefineSymbolsForGroup(platform, "");
			Debug.Log("Compiler Directives cleard");
		}

		[MenuItem("Edit/Reign/Platform Tools/Disable Reign")]
		private static void DisableReignForPlatform()
		{
			applyCompilerDirectives(true, "DISABLE_REIGN");
		}
		
		[MenuItem("Edit/Reign/Platform Tools/Enable Reign")]
		private static void EnableReignForPlatform()
		{
			removeCompilerDirectives("DISABLE_REIGN");
		}
		#endregion
	}
}