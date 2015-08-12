using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Xml;
using System.Xml.Serialization;

//https://developer.blackberry.com/native/documentation/core/com.qnx.doc.scoreloop.lib_ref/topic/coresocial-achievements.html
namespace Injector
{
	#region XML
	namespace OldUnityXml
	{
		public class env
		{
			[XmlAttribute("var")] public string _var;
			[XmlAttribute("value")] public string value;
		}

		public class icon
		{
			[XmlElement("image")] public string image;
		}

		public class splashScreens
		{
			[XmlElement("image")] public string[] images;
		}

		public class initialWindow
		{
			[XmlElement("autoOrients")] public string autoOrients;
			[XmlElement("aspectRatio")] public string aspectRatio;
			[XmlElement("systemChrome")] public string systemChrome;
			[XmlElement("transparent")] public string transparent;
		}

		public class action
		{
			[XmlAttribute("system")] public string system;
			[XmlText] public string content;
		}

		[XmlRoot("qnx")]
		public class qnx
		{
			//[XmlAttribute("xmlns")] public string xmlns;
			[XmlElement("env")] public env env;

			[XmlElement("author")] public string author;
			[XmlElement("authorId")] public string authorId;
			[XmlElement("id")] public string id;
			[XmlElement("filename")] public string filename;
			[XmlElement("name")] public string name;
			[XmlElement("description")] public string description;
			[XmlElement("publisher")] public string publisher;
			[XmlElement("versionNumber")] public string versionNumber;

			[XmlElement("icon")] public icon icon;
			[XmlElement("splashScreens")] public splashScreens splashScreens;
			[XmlElement("initialWindow")] public initialWindow initialWindow;

			[XmlElement("category")] public string category;
			[XmlElement("action")] public action[] actions;
		}
	}

	namespace NewUnityXml
	{
		public class env
		{
			[XmlAttribute("var")] public string _var;
			[XmlAttribute("value")] public string value;

			public env() {}
			public env(env old)
			{
				_var = old._var;
				value = old.value;
			}

			public env(OldUnityXml.env old)
			{
				_var = old._var;
				value = old.value;
			}
		}

		public class asset
		{
			[XmlAttribute("path")] public string path;
			[XmlText] public string content;

			public asset() {}
			public asset(string path, string content)
			{
				this.path = path;
				this.content = content;
			}
		}

		public class icon
		{
			[XmlElement("image")] public string image;

			public icon() {}
			public icon(icon old)
			{
				image = old.image;
			}

			public icon(OldUnityXml.icon old)
			{
				image = old.image;
			}
		}

		public class splashScreens
		{
			[XmlElement("image")] public string[] images;

			public splashScreens() {}
			public splashScreens(splashScreens old)
			{
				if (old.images == null) return;

				images = new string[old.images.Length];
				for (int i = 0; i != images.Length; ++i)
				{
					images[i] = old.images[i];
				}
			}

			public splashScreens(OldUnityXml.splashScreens old)
			{
				if (old.images == null) return;

				images = new string[old.images.Length];
				for (int i = 0; i != images.Length; ++i)
				{
					images[i] = old.images[i];
				}
			}
		}

		public class initialWindow
		{
			[XmlElement("autoOrients")] public string autoOrients;
			[XmlElement("aspectRatio")] public string aspectRatio;
			[XmlElement("systemChrome")] public string systemChrome;
			[XmlElement("transparent")] public string transparent;

			public initialWindow() {}
			public initialWindow(initialWindow old)
			{
				autoOrients = old.autoOrients;
				aspectRatio = old.aspectRatio;
				systemChrome = old.systemChrome;
				transparent = old.transparent;
			}

			public initialWindow(OldUnityXml.initialWindow old)
			{
				autoOrients = old.autoOrients;
				aspectRatio = old.aspectRatio;
				systemChrome = old.systemChrome;
				transparent = old.transparent;
			}
		}

		public class configuration_asset
		{
			[XmlAttribute("path")] public string path;
			[XmlAttribute("entry")] public string entry;
			[XmlAttribute("type")] public string type;
			[XmlText] public string content;

			public configuration_asset() {}
			public configuration_asset(configuration_asset old)
			{
				path = old.path;
				entry = "true";
				type = "Qnx/Elf";
				content = old.content;
			}

			public configuration_asset(OldUnityXml.qnx old)
			{
				path = old.filename;
				entry = "true";
				type = "Qnx/Elf";
				content = old.filename;
			}
		}

		public class configuration
		{
			[XmlAttribute("name")] public string name;
			[XmlElement("platformArchitecture")] public string platformArchitecture;
			[XmlElement("asset")] public configuration_asset asset;

			public configuration() {}
			public configuration(configuration old)
			{
				name = "Device-Release";
				platformArchitecture = "armle-v7";
				asset = old.asset;
			}

			public configuration(OldUnityXml.qnx old)
			{
				name = "Device-Release";
				platformArchitecture = "armle-v7";
				asset = new configuration_asset(old);
			}
		}

		public class permission
		{
			[XmlAttribute("system")] public string system;
			[XmlText] public string content;

			public permission() {}
			public permission(permission old)
			{
				system = old.system;
				content = old.content;
			}

			public permission(OldUnityXml.action old)
			{
				system = old.system;
				content = old.content;
			}
		}

		[XmlRoot("qnx")]
		public class qnx
		{
			//[XmlAttribute("xmlns")] public string xmlns;
			[XmlElement("env")] public env env;

			[XmlElement("author")] public string author;
			[XmlElement("authorId")] public string authorId;
			[XmlElement("id")] public string id;
			[XmlElement("filename")] public string filename;
			[XmlElement("name")] public string name;
			[XmlElement("description")] public string description;
			[XmlElement("publisher")] public string publisher;
			[XmlElement("versionNumber")] public string versionNumber;

			[XmlElement("asset")] public asset[] assets;// <<<
			[XmlElement("icon")] public icon icon;
			[XmlElement("splashScreens")] public splashScreens splashScreens;
			[XmlElement("initialWindow")] public initialWindow initialWindow;
			[XmlElement("configuration")] public configuration configuration;// <<<

			[XmlElement("category")] public string category;
			[XmlElement("permission")] public permission[] permissions;// <<<

			public qnx() {}
			public qnx(qnx old)
			{
				env = new env(old.env);

				author = old.author;
				authorId = old.authorId;
				id = old.id;
				filename = old.filename;
				name = old.name;
				description = old.description;
				publisher = old.publisher;
				versionNumber = old.versionNumber;

				int assetCount = 5, splashAssetCount = old.splashScreens.images != null ? old.splashScreens.images.Length : 0;
				assets = new asset[assetCount + splashAssetCount];
				assets[0] = new asset(old.icon.image, old.icon.image);
				assets[1] = new asset("Data", null);
				assets[2] = new asset("lib", null);
				assets[3] = new asset("SLAwards.bundle", "scoreloop/SLAwards.bundle");
				assets[4] = new asset("Release", null);
				for (int i = 0; i != splashAssetCount; ++i)
				{
					assets[assetCount+i] = new asset(old.splashScreens.images[i], old.splashScreens.images[i]);
				}

				icon = new icon(old.icon);
				splashScreens = new splashScreens(old.splashScreens);
				initialWindow = new initialWindow(old.initialWindow);
				if (old.configuration != null) configuration = new configuration(old.configuration);

				category = old.category;
				permissions = new permission[old.permissions.Length];
				for (int i = 0; i != permissions.Length; ++i)
				{
					permissions[i] = new permission(old.permissions[i]);
				}
			}

			public qnx(OldUnityXml.qnx old)
			{
				env = new env(old.env);

				author = old.author;
				authorId = old.authorId;
				id = old.id;
				filename = old.filename;
				name = old.name;
				description = old.description;
				publisher = old.publisher;
				versionNumber = old.versionNumber;

				int assetCount = 5, splashAssetCount = old.splashScreens.images != null ? old.splashScreens.images.Length : 0;
				assets = new asset[assetCount + splashAssetCount];
				assets[0] = new asset(old.icon.image, old.icon.image);
				assets[1] = new asset("Data", null);
				assets[2] = new asset("lib", null);
				assets[3] = new asset("SLAwards.bundle", "scoreloop/SLAwards.bundle");
				assets[4] = new asset("Release", null);
				for (int i = 0; i != splashAssetCount; ++i)
				{
					assets[assetCount+i] = new asset(old.splashScreens.images[i], old.splashScreens.images[i]);
				}

				icon = new icon(old.icon);
				splashScreens = new splashScreens(old.splashScreens);
				initialWindow = new initialWindow(old.initialWindow);
				configuration = new configuration(old);

				category = old.category;
				permissions = new permission[old.actions.Length];
				for (int i = 0; i != permissions.Length; ++i)
				{
					permissions[i] = new permission(old.actions[i]);
				}
			}
		}
	}
	#endregion

	static class InjectorCore
	{
		public static string BarFileName = "", ScoreloopBundlePath = "", UnityPath = "", KeyPassword = "Key password...", PhoneIP = "0.0.0.0", PhonePass = "XXXX";
		public static bool SignBarFile, UseClassicMode;
		private static FileStream logStream;
		private static StreamWriter logWriter;

		public static void FindUnityPath()
		{
			#if WIN32
			string path1 = @"C:\Program Files (x86)\Unity";
			if (Directory.Exists(path1))
			{
				UnityPath = path1;
				return;
			}

			string path2 = @"C:\Program Files\Unity";
			if (Directory.Exists(path2))
			{
				UnityPath = path2;
				return;
			}

			UnityPath = path1;
			#else
			UnityPath = "/Applications/Unity/Unity.app";
			#endif
		}

		public static void LoadPaths()
		{
			try
			{
				#if OSX
				string path = Path.GetDirectoryName(MonoMac.Foundation.NSBundle.MainBundle.BundlePath);
				using (var file = new FileStream(path+"/Settings.txt", FileMode.Open, FileAccess.Read))
				#else
				using (var file = new FileStream("Settings.txt", FileMode.Open, FileAccess.Read))
				#endif
				using (var reader = new StreamReader(file))
				{
					BarFileName = reader.ReadLine();
					ScoreloopBundlePath = reader.ReadLine();
					KeyPassword = reader.ReadLine();
					SignBarFile = bool.Parse(reader.ReadLine());

					PhoneIP = reader.ReadLine();
					PhonePass = reader.ReadLine();
				}
			}
			catch
			{
				// do nothing...
			}
		}

		public static void SavePaths()
		{
			try
			{
				#if OSX
				string path = Path.GetDirectoryName(MonoMac.Foundation.NSBundle.MainBundle.BundlePath);
				using (var file = new FileStream(path+"/Settings.txt", FileMode.Create, FileAccess.Write))
				#else
				using (var file = new FileStream("Settings.txt", FileMode.Create, FileAccess.Write))
				#endif
				using (var writer = new StreamWriter(file))
				{
					writer.WriteLine(BarFileName);
					writer.WriteLine(ScoreloopBundlePath);
					writer.WriteLine(KeyPassword);
					writer.WriteLine(SignBarFile.ToString());

					writer.WriteLine(PhoneIP);
					writer.WriteLine(PhonePass);
				}
			}
			catch
			{
				// do nothing...
			}
		}

		public static string ProcessBarFile()
		{
			try
			{
				// extract file
				string path = Path.GetDirectoryName(BarFileName);
				string extractPath = path + "/" + Path.GetFileNameWithoutExtension(BarFileName);
				if (Directory.Exists(extractPath)) Directory.Delete(extractPath, true);
				if (!Directory.Exists(extractPath)) Directory.CreateDirectory(extractPath);
				#if WIN32
				ZipFile.ExtractToDirectory(BarFileName, extractPath);
				#else
				using (var zip = Ionic.Zip.ZipFile.Read(BarFileName))
				{
					zip.ExtractAll(extractPath);
				}
				#endif
				
				// delete old meta data
				if (Directory.Exists(extractPath+"/META-INF")) Directory.Delete(extractPath+"/META-INF", true);

				// create directories
				if (!Directory.Exists(extractPath+"/native")) Directory.CreateDirectory(extractPath+"/native");

				// copy scoreloop files
				if (!string.IsNullOrEmpty(ScoreloopBundlePath))
				{
					path = extractPath + "/native/SLAwards.bundle";
					DirectoryCopy(ScoreloopBundlePath, path, true);
				}

				// update xml manifest
				if (UseClassicMode)
				{
					var serializer = new XmlSerializer(typeof(OldUnityXml.qnx), "http://www.BB10.com/schemas/application/1.0");
					OldUnityXml.qnx oldObj = null;
					using (var file = new FileStream(extractPath+"/native/bar-descriptor.xml", FileMode.Open, FileAccess.Read))
					{
						oldObj = (OldUnityXml.qnx)serializer.Deserialize(file);
					}

					if (oldObj != null)
					{
						serializer = new XmlSerializer(typeof(NewUnityXml.qnx), "http://www.qnx.com/schemas/application/1.0");
						var newObj = new NewUnityXml.qnx(oldObj);
						using (var file = new FileStream(extractPath+"/native/bar-descriptor.xml", FileMode.Create, FileAccess.Write))
						{
							serializer.Serialize(file, newObj);
						}
					}
					else
					{
						throw new Exception("Failed to Deserialize bar-descriptor.xml");
					}
				}
				else
				{
					var serializer = new XmlSerializer(typeof(NewUnityXml.qnx), "http://www.BB10.com/schemas/application/1.0");
					NewUnityXml.qnx oldObj = null;
					using (var file = new FileStream(extractPath+"/native/bar-descriptor.xml", FileMode.Open, FileAccess.Read))
					{
						oldObj = (NewUnityXml.qnx)serializer.Deserialize(file);
					}

					if (oldObj != null)
					{
						serializer = new XmlSerializer(typeof(NewUnityXml.qnx), "http://www.qnx.com/schemas/application/1.0");
						var newObj = new NewUnityXml.qnx(oldObj);
						using (var file = new FileStream(extractPath+"/native/bar-descriptor.xml", FileMode.Create, FileAccess.Write))
						{
							serializer.Serialize(file, newObj);
						}
					}
					else
					{
						throw new Exception("Failed to Deserialize bar-descriptor.xml");
					}
				}

				// repackage bar file >>>
				// <<< set java variables for windows
				#if WIN32
				Environment.SetEnvironmentVariable("JAVA_HOME", @"C:\Program Files (x86)\Java\jre7\bin");
				#endif

				// <<< create log file
				createLogFile();

				// get tool path
				string invokePath;
				if(UseClassicMode)
				{
					#if WIN32
					invokePath = UnityPath + @"\Editor\Data\PlaybackEngines\bb10player\blackberry-tools\bin";
					#else
					invokePath = UnityPath + "/Contents/PlaybackEngines/BB10Player/blackberry-tools/bin";
					#endif
				}
				else
				{
					#if WIN32
					invokePath = UnityPath + @"\Editor\Data\PlaybackEngines\blackberryplayer\blackberry-tools\bin";
					#else
					invokePath = UnityPath + "/Contents/PlaybackEngines/BlackBerryPlayer/Tools/blackberry-tools/bin";
					#endif
				}

				// <<< run packager
				string exe = "blackberry-nativepackager";
				#if WIN32
				exe += ".bat";
				#endif
				logWriter.WriteLine("Running " + exe);
				var info = new ProcessStartInfo(invokePath+"/"+exe, string.Format(@"{2}-package ""{0}"" ""{1}""", extractPath+"_Output.bar", extractPath+"/native/bar-descriptor.xml", !SignBarFile ? "-devMode " : ""));
				info.RedirectStandardOutput = true;
				info.RedirectStandardError = true;
				info.UseShellExecute = false;
				info.WorkingDirectory = extractPath+"/native";
				var process = new Process();
				process.StartInfo = info;
				process.OutputDataReceived += process_OutputDataReceived;
				process.ErrorDataReceived += process_ErrorDataReceived;
				if (!process.Start()) throw new Exception("Failed to start "+exe);
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();
				process.WaitForExit();

				// <<< run signer
				if (SignBarFile)
				{
					exe = "blackberry-signer";
					#if WIN32
					exe += ".bat";
					#endif
					logWriter.WriteLine("Running " + exe);
					info = new ProcessStartInfo(invokePath+"/"+exe, string.Format(@"-storepass {0} ""{1}""", KeyPassword, extractPath+"_Output.bar"));
					info.RedirectStandardOutput = true;
					info.RedirectStandardError = true;
					info.UseShellExecute = false;
					process = new Process();
					process.StartInfo = info;
					process.OutputDataReceived += process_OutputDataReceived;
					process.ErrorDataReceived += process_ErrorDataReceived;
					if (!process.Start()) throw new Exception("Failed to start "+exe);
					process.BeginOutputReadLine();
					process.BeginErrorReadLine();
					process.WaitForExit();
				}
			}
			catch (Exception e)
			{
				if (logStream != null)
				{
					logWriter.Close();
					logStream.Close();
					logStream = null;
				}
				return e.Message;
			}
			
			if (logStream != null)
			{
				logWriter.Close();
				logStream.Close();
				logStream = null;
			}

			return null;
		}

		static void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
		{
			logWriter.WriteLine(e.Data);
		}
		
		private static void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			logWriter.WriteLine(e.Data);
		}

		private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
		{
			// Get the subdirectories for the specified directory.
			DirectoryInfo dir = new DirectoryInfo(sourceDirName);
			DirectoryInfo[] dirs = dir.GetDirectories();

			if (!dir.Exists)
			{
				throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
			}

			// If the destination directory doesn't exist, create it. 
			if (!Directory.Exists(destDirName))
			{
				Directory.CreateDirectory(destDirName);
			}

			// Get the files in the directory and copy them to the new location.
			FileInfo[] files = dir.GetFiles();
			foreach (FileInfo file in files)
			{
				string temppath = Path.Combine(destDirName, file.Name);
				file.CopyTo(temppath, true);
			}

			// If copying subdirectories, copy them and their contents to new location. 
			if (copySubDirs)
			{
				foreach (DirectoryInfo subdir in dirs)
				{
					string temppath = Path.Combine(destDirName, subdir.Name);
					DirectoryCopy(subdir.FullName, temppath, copySubDirs);
				}
			}
		}

		private static void createLogFile()
		{
			string logPath = "";
			#if OSX
			logPath = Path.GetDirectoryName(MonoMac.Foundation.NSBundle.MainBundle.BundlePath) + "/";
			#endif
			logStream = new FileStream(logPath+"log.txt", FileMode.Create, FileAccess.Write);
			logWriter = new StreamWriter(logStream);
		}

		public static string Upload(string barFile)
		{
			try
			{
				// create log file
				createLogFile();

				// get tool path
				string invokePath;
				if (UseClassicMode)
				{
					#if WIN32
					invokePath = UnityPath + @"\Editor\Data\PlaybackEngines\bb10player\blackberry-tools\bin";
					#else
					invokePath = UnityPath + "/Contents/PlaybackEngines/BB10Player/blackberry-tools/bin";
					#endif
				}
				else
				{
					#if WIN32
					invokePath = UnityPath + @"\Editor\Data\PlaybackEngines\blackberryplayer\blackberry-tools\bin";
					#else
					invokePath = UnityPath + "/Contents/PlaybackEngines/BlackBerryPlayer/Tools/blackberry-tools/bin";
					#endif
				}

				// run packager
				string exe = "blackberry-deploy";
				#if WIN32
				exe += ".bat";
				#endif
				logWriter.WriteLine("Running " + exe);
				var info = new ProcessStartInfo(invokePath+"/"+exe, string.Format(@"-installApp -package ""{0}"" -device {1} -password {2}", barFile, PhoneIP, PhonePass));
				info.RedirectStandardOutput = true;
				info.RedirectStandardError = true;
				info.UseShellExecute = false;
				//info.WorkingDirectory = extractPath+"/native";
				var process = new Process();
				process.StartInfo = info;
				process.OutputDataReceived += process_OutputDataReceived;
				process.ErrorDataReceived += process_ErrorDataReceived;
				if (!process.Start()) throw new Exception("Failed to start "+exe);
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();
				process.WaitForExit();
			}
			catch (Exception e)
			{
				if (logStream != null)
				{
					logWriter.Close();
					logStream.Close();
					logStream = null;
				}
				return e.Message;
			}
			
			if (logStream != null)
			{
				logWriter.Close();
				logStream.Close();
				logStream = null;
			}

			return null;
		}
	}
}
