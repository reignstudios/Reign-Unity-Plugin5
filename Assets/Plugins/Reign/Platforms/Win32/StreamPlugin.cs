#if UNITY_STANDALONE_WIN
using UnityEngine;
using System.Collections;
using System.IO;
using ImageTools.IO.Png;
using ImageTools.Filtering;
using ImageTools;
using System;
using System.Runtime.InteropServices;

namespace Reign.Plugin
{
	struct OPENFILENAME
	{
		public uint         lStructSize;
		public IntPtr          hwndOwner;
		public IntPtr     hInstance;
		public IntPtr       lpstrFilter;
		public IntPtr        lpstrCustomFilter;
		public uint         nMaxCustFilter;
		public uint         nFilterIndex;
		public IntPtr        lpstrFile;
		public uint         nMaxFile;
		public IntPtr        lpstrFileTitle;
		public uint         nMaxFileTitle;
		public IntPtr       lpstrInitialDir;
		public IntPtr       lpstrTitle;
		public uint         Flags;
		public Int16          nFileOffset;
		public Int16          nFileExtension;
		public IntPtr       lpstrDefExt;
		public IntPtr        lCustData;
		public IntPtr lpfnHook;
		public IntPtr       lpTemplateName;
		public IntPtr          pvReserved;
		public uint         dwReserved;
		public uint         FlagsEx;
	};

	public class StreamPlugin_Win32 : StreamPluginBase
	{
		[DllImport("Comdlg32.dll", EntryPoint="GetOpenFileNameW")]
		private extern static void GetOpenFileName(ref OPENFILENAME lpofn);

		[DllImport("Comdlg32.dll", EntryPoint="GetSaveFileNameW")]
		private extern static void GetSaveFileName(ref OPENFILENAME lpofn);

		private const uint MAX_PATH = 0x00000104;
		private const uint OFN_PATHMUSTEXIST = 0x00000800, OFN_FILEMUSTEXIST = 0x00001000, OFN_NOCHANGEDIR = 0x00000008;

		private static IntPtr generateFilterValue(string[] fileTypes)
		{
			string filterValue = "File Types (";
			foreach (var type in fileTypes)
			{
				filterValue += "*" + type;
				if (type != fileTypes[fileTypes.Length-1]) filterValue += ";";
			}
			filterValue += ")\0";
			foreach (var type in fileTypes)
			{
				filterValue += "*" + type;
				if (type != fileTypes[fileTypes.Length-1]) filterValue += ";";
			}
			filterValue += "\0\0";

			IntPtr filterPtr = Marshal.AllocHGlobal((filterValue.Length*2) + 1);
			var filterData = new char[filterValue.Length+1];
			for (int i = 0; i != filterValue.Length; ++i) filterData[i] = (char)filterValue[i];
			Marshal.Copy(filterData, 0, filterPtr, filterValue.Length);

			return filterPtr;
		}

		public override void SaveFileDialog(Stream stream, FolderLocations folderLocation, string[] fileTypes, StreamSavedCallbackMethod streamSavedCallback)
		{
			if (streamSavedCallback == null) return;

			var data = new byte[stream.Length];
			stream.Position = 0;
			stream.Read(data, 0, data.Length);
			SaveFileDialog(data, folderLocation, fileTypes, streamSavedCallback);
		}

		public override void SaveFileDialog(byte[] data, FolderLocations folderLocation, string[] fileTypes, StreamSavedCallbackMethod streamSavedCallback)
		{
			if (streamSavedCallback == null) return;

			// open native dlg
			var file = new OPENFILENAME();
			file.lStructSize = (uint)Marshal.SizeOf(typeof(OPENFILENAME));
			file.hwndOwner = IntPtr.Zero;
			file.lpstrDefExt = IntPtr.Zero;
			file.lpstrFile = Marshal.AllocHGlobal((int)MAX_PATH);
			unsafe {((byte*)file.lpstrFile.ToPointer())[0] = 0;}
			file.nMaxFile = MAX_PATH;
			file.lpstrFilter = generateFilterValue(fileTypes);
			file.nFilterIndex = 0;
			file.lpstrInitialDir = Marshal.StringToHGlobalUni(Application.dataPath);
			file.lpstrTitle = Marshal.StringToHGlobalUni("Save file");
			file.Flags = OFN_PATHMUSTEXIST | OFN_FILEMUSTEXIST | OFN_NOCHANGEDIR;
			GetSaveFileName(ref file);

			// get native dlg result
			string filename = null;
			if (file.lpstrFile != IntPtr.Zero)
			{
				filename = Marshal.PtrToStringUni(file.lpstrFile);
				Debug.Log("Saving file: " + filename);
			}

			Marshal.FreeHGlobal(file.lpstrFile);
			Marshal.FreeHGlobal(file.lpstrInitialDir);
			Marshal.FreeHGlobal(file.lpstrTitle);
			Marshal.FreeHGlobal(file.lpstrFilter);

			// save file
			if (!string.IsNullOrEmpty(filename))
			{
				using (var stream = new FileStream(filename, FileMode.Create, FileAccess.Write))
				{
					stream.Write(data, 0, data.Length);
				}

				if (streamSavedCallback != null) streamSavedCallback(true);
			}
			else
			{
				if (streamSavedCallback != null) streamSavedCallback(false);
			}
		}

		public override void LoadFileDialog(FolderLocations folderLocation, int maxWidth, int maxHeight, int x, int y, int width, int height, string[] fileTypes, StreamLoadedCallbackMethod streamLoadedCallback)
		{
			if (streamLoadedCallback == null) return;

			// open native dlg
			var file = new OPENFILENAME();
			file.lStructSize = (uint)Marshal.SizeOf(typeof(OPENFILENAME));
			file.hwndOwner = IntPtr.Zero;
			file.lpstrDefExt = IntPtr.Zero;
			file.lpstrFile = Marshal.AllocHGlobal((int)MAX_PATH);
			unsafe {((byte*)file.lpstrFile.ToPointer())[0] = 0;}
			file.nMaxFile = MAX_PATH;
			file.lpstrFilter = generateFilterValue(fileTypes);
			file.nFilterIndex = 0;
			file.lpstrInitialDir = Marshal.StringToHGlobalUni(Application.dataPath);
			file.lpstrTitle = Marshal.StringToHGlobalUni("Load file");
			file.Flags = OFN_PATHMUSTEXIST | OFN_FILEMUSTEXIST | OFN_NOCHANGEDIR;
			GetOpenFileName(ref file);

			// get native dlg result
			string filename = null;
			if (file.lpstrFile != IntPtr.Zero)
			{
				filename = Marshal.PtrToStringUni(file.lpstrFile);
				Debug.Log("Loading file: " + filename);
			}

			Marshal.FreeHGlobal(file.lpstrFile);
			Marshal.FreeHGlobal(file.lpstrInitialDir);
			Marshal.FreeHGlobal(file.lpstrTitle);
			Marshal.FreeHGlobal(file.lpstrFilter);

			// open file
			if (!string.IsNullOrEmpty(filename))
			{
				if (maxWidth == 0 || maxHeight == 0 || folderLocation != FolderLocations.Pictures)
				{
					streamLoadedCallback(new FileStream(filename, FileMode.Open, FileAccess.Read), true);
				}
				else
				{
					var newStream = new MemoryStream();
					try
					{
						using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
						{
							ImageTools.IO.IImageDecoder decoder = null;
							switch (Path.GetExtension(filename).ToLower())
							{
								case ".jpg": decoder = new ImageTools.IO.Jpeg.JpegDecoder(); break;
								case ".jpeg": decoder = new ImageTools.IO.Jpeg.JpegDecoder(); break;
								case ".png": decoder = new ImageTools.IO.Png.PngDecoder(); break;
								default:
									Debug.LogError("Unsuported file ext type: " + Path.GetExtension(filename));
									streamLoadedCallback(null, false);
									return;
							}
							var image = new ExtendedImage();
							decoder.Decode(image, stream);
							var newSize = Reign.MathUtilities.FitInViewIfLarger(image.PixelWidth, image.PixelHeight, maxWidth, maxHeight);
							var newImage = ExtendedImage.Resize(image, (int)newSize.x, (int)newSize.y, new NearestNeighborResizer());

							var encoder = new PngEncoder();
							encoder.Encode(newImage, newStream);
							newStream.Position = 0;
						}
					}
					catch (Exception e)
					{
						newStream.Dispose();
						newStream = null;
						Debug.LogError(e.Message);
					}
					finally
					{
						streamLoadedCallback(newStream, true);
					}
				}
			}
			else
			{
				streamLoadedCallback(null, false);
			}
		}
	}
}
#endif