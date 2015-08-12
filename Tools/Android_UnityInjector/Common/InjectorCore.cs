using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Xml;
using System.Xml.Serialization;

namespace Injector
{
	static class InjectorCore
	{
		public static string ApkFileName = "", AndroidSDKPath = "", UnityPath = "", KeyPassword = "Key password...", PhoneIP = "0.0.0.0";
		public static bool SignApkFile;
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
					ApkFileName = reader.ReadLine();
					AndroidSDKPath = reader.ReadLine();
					KeyPassword = reader.ReadLine();
					SignApkFile = bool.Parse(reader.ReadLine());

					PhoneIP = reader.ReadLine();
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
					writer.WriteLine(ApkFileName);
					writer.WriteLine(AndroidSDKPath);
					writer.WriteLine(KeyPassword);
					writer.WriteLine(SignApkFile.ToString());

					writer.WriteLine(PhoneIP);
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
				// create log file
				createLogFile();
				ProcessStartInfo info = null;
				Process process = null;

				// merge files into apk
				string path = Path.GetDirectoryName(ApkFileName);
				string apkCopy = path + "\\" + Path.GetFileNameWithoutExtension(ApkFileName) + "_Injected.apk";
				//File.Copy(ApkFileName, apkCopy, true);

				string invokePath = AndroidSDKPath + "/build-tools/android-4.4";
				/*string exe = "aapt";
				#if WIN32
				exe += ".exe";
				#endif
				logWriter.WriteLine("Running " + exe);
				DirectoryCopy(AndroidSDKPath + "/extras/google/google_play_services/libproject/google-play-services_lib/res", path + "/res", true);

				var files = new List<string>();
				buildFileList(path, path + "/res", files);
				foreach (var file in files)
				{
					info = new ProcessStartInfo(invokePath+"/"+exe, string.Format(@"add ""{0}"" ""{1}""", apkCopy, file));
					info.RedirectStandardOutput = true;
					info.RedirectStandardError = true;
					info.UseShellExecute = false;
					info.WorkingDirectory = path;
					process = new Process();
					process.StartInfo = info;
					process.OutputDataReceived += process_OutputDataReceived;
					process.ErrorDataReceived += process_ErrorDataReceived;
					if (!process.Start()) throw new Exception("Failed to start "+exe);
					process.BeginOutputReadLine();
					process.BeginErrorReadLine();
					process.WaitForExit();
				}*/

				// repackage bar file >>>
				// <<< set java variables for windows
				#if WIN32
				//Environment.SetEnvironmentVariable("JAVA_HOME", @"C:\Program Files (x86)\Java\jre7\bin");
				Environment.SetEnvironmentVariable("JAVA_HOME", @"C:\Program Files\Java\jdk1.7.0_45\bin");
				#endif

				info = new ProcessStartInfo(@"C:\Program Files\Java\jdk1.7.0_45\bin\jarsigner", string.Format(@"-verbose -sigalg SHA1withRSA -digestalg SHA1 -keystore ""{0}"" ""{1}"" androiddebugkey", @"C:\Users\Andrew\.android\debug.keystore", apkCopy));
				info.RedirectStandardOutput = true;
				info.RedirectStandardError = true;
				info.RedirectStandardInput = true;
				info.UseShellExecute = false;
				info.WorkingDirectory = path;
				process = new Process();
				process.StartInfo = info;
				process.OutputDataReceived += process_OutputDataReceived;
				process.ErrorDataReceived += process_ErrorDataReceived;
				if (!process.Start()) throw new Exception("Failed to start jarsigner");
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();
				process.StandardInput.WriteLine("android");
				process.WaitForExit();

				// zip align
				info = new ProcessStartInfo(@"C:\Users\Andrew\SDKs\adt-bundle-windows-x86_64-20131030\sdk\tools\zipalign", string.Format(@"-v 4 ""{0}"" aligned.apk", apkCopy));
				info.RedirectStandardOutput = true;
				info.RedirectStandardError = true;
				info.RedirectStandardInput = true;
				info.UseShellExecute = false;
				info.WorkingDirectory = path;
				process = new Process();
				process.StartInfo = info;
				process.OutputDataReceived += process_OutputDataReceived;
				process.ErrorDataReceived += process_ErrorDataReceived;
				if (!process.Start()) throw new Exception("Failed to start jarsigner");
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

		private static void buildFileList(string rootPath, string path, List<string> fileNames)
		{
			DirectoryInfo dir = new DirectoryInfo(path);
			DirectoryInfo[] dirs = dir.GetDirectories();

			if (!dir.Exists)
			{
				throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + path);
			}

			FileInfo[] files = dir.GetFiles();
			foreach (FileInfo file in files)
			{
				fileNames.Add(file.FullName.Substring(rootPath.Length+1, file.FullName.Length-rootPath.Length-1));
			}

			foreach (DirectoryInfo subdir in dirs)
			{
				buildFileList(rootPath, subdir.FullName, fileNames);
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

		public static string Upload(string apkFile)
		{
			/*try
			{
				// create log file
				createLogFile();

				// get tool path
				#if WIN32
				string invokePath = UnityPath + @"\Editor\Data\PlaybackEngines\bb10player\blackberry-tools\bin";
				#else
				string invokePath = UnityPath + "/Contents/PlaybackEngines/BB10Player/blackberry-tools/bin";
				#endif

				// run packager
				string exe = "blackberry-deploy";
				#if WIN32
				exe += ".bat";
				#endif
				logWriter.WriteLine("Running " + exe);
				var info = new ProcessStartInfo(invokePath+"/"+exe, string.Format(@"-installApp -package ""{0}"" -device {1}", apkFile, PhoneIP));
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
			}*/

			return null;
		}
	}
}
