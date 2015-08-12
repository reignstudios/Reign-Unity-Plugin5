#if REIGN_POSTBUILD
using System;
using System.IO;
using Windows.ApplicationModel;
using Windows.Storage;
using UnityEngine;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using System.Collections.Generic;
using Windows.UI.Core;
using System.Linq;

#if UNITY_METRO
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.Media.Capture;
#endif

#if WINDOWS_PHONE
using Microsoft.Xna.Framework.Media;
using Microsoft.Phone.Tasks;
using System.Windows.Media.Imaging;
#endif

namespace Reign.Plugin
{
	public class StreamPlugin_Native : IStreamPlugin
	{
		#if WINDOWS_PHONE
		private PhotoChooserTask photoChooser;
		#endif

		public void Update()
		{
			// do nothing...
		}

		public void FileExists(string fileName, FolderLocations folderLocation, StreamExistsCallbackMethod callback)
		{
			if (callback == null) return;
			if (folderLocation != FolderLocations.Storage)
			{
				Debug.LogError("FileExists Error: Only Storage folder location is currently supported.");
				callback(false);
				return;
			}

			fileExistsAsync(fileName, callback);
		}

		private async void fileExistsAsync(string fileName, StreamExistsCallbackMethod callback)
		{
			try
			{
				var storageFolder = ApplicationData.Current.LocalFolder;
				var file = await storageFolder.GetFileAsync(fileName);
			}
			catch (FileNotFoundException)
			{
				callback(false);
				return;
			}
			catch (Exception e)
			{
				Debug.LogError("FileExists Error: " + e.Message);
				callback(false);
				return;
			}

			callback(true);
		}

		public void DeleteFile(string fileName, FolderLocations folderLocation, StreamDeleteCallbackMethod callback)
		{
			if (folderLocation != FolderLocations.Storage)
			{
				Debug.LogError("DeleteFile Error: Only Storage folder location is currently supported.");
				callback(false);
				return;
			}

			deleteFileAsync(fileName, callback);
		}

		private async void deleteFileAsync(string fileName, StreamDeleteCallbackMethod callback)
		{
			try
			{
				var storageFolder = ApplicationData.Current.LocalFolder;
				var file = await storageFolder.GetFileAsync(fileName);
				await file.DeleteAsync();
			}
			catch (Exception e)
			{
				Debug.LogError("DeleteFile Error: " + e.Message);
				callback(false);
				return;
			}

			callback(true);
		}

		public void SaveFile(string fileName, Stream stream, FolderLocations folderLocation, StreamSavedCallbackMethod streamSavedCallback)
		{
			var data = new byte[stream.Length];
			stream.Read(data, 0, data.Length);
			SaveFile(fileName, data, folderLocation, streamSavedCallback);
		}

		public void SaveFile(string fileName, byte[] data, FolderLocations folderLocation, StreamSavedCallbackMethod streamSavedCallback)
		{
			saveFileAsync(fileName, data, folderLocation, streamSavedCallback);
		}

		private async Task<Stream> saveStream(string fileName, FolderLocations folderLocation)
		{
			switch (folderLocation)
			{
				case FolderLocations.Application:
					var appFolder = Package.Current.InstalledLocation;
					return await appFolder.OpenStreamForWriteAsync(fileName, CreationCollisionOption.ReplaceExisting);

				case FolderLocations.Storage:
					var storageFolder = ApplicationData.Current.LocalFolder;
					return await storageFolder.OpenStreamForWriteAsync(fileName, CreationCollisionOption.ReplaceExisting);

				case FolderLocations.Documents:
					var docFile = await KnownFolders.DocumentsLibrary.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
					return await docFile.OpenStreamForWriteAsync();

				case FolderLocations.Pictures:
					var picFile = await KnownFolders.PicturesLibrary.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
					return await picFile.OpenStreamForWriteAsync();

				case FolderLocations.Music:
					var musicFile = await KnownFolders.MusicLibrary.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
					return await musicFile.OpenStreamForWriteAsync();

				case FolderLocations.Video:
					var videoFile = await KnownFolders.VideosLibrary.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
					return await videoFile.OpenStreamForWriteAsync();

				default:
					Debug.LogError("Unsuported folder location: " + folderLocation);
					return null;
			}
		}

		private async void saveFileAsync(string fileName, byte[] data, FolderLocations folderLocation, StreamSavedCallbackMethod streamSavedCallback)
		{
			try
			{
				#if WINDOWS_PHONE
				if (folderLocation == FolderLocations.Pictures)
				{
					using (var m = new MediaLibrary())
					using (var image = m.SavePicture(fileName, data))
					{
						if (streamSavedCallback != null) streamSavedCallback(image != null);
					}
					return;
				}
				#endif
				
				fileName = fileName.Replace('/', '\\');
				using (var stream = await saveStream(fileName, folderLocation))
				{
					stream.Write(data, 0, data.Length);
				}
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message);
				if (streamSavedCallback != null) streamSavedCallback(false);
				return;
			}

			if (streamSavedCallback != null) streamSavedCallback(true);
		}

		public void LoadFile(string fileName, FolderLocations folderLocation, StreamLoadedCallbackMethod streamLoadedCallback)
		{
			if (streamLoadedCallback == null) return;
			loadFileAsync(fileName, folderLocation, streamLoadedCallback);
		}

		private async Task<Stream> openStream(string fileName, FolderLocations folderLocation)
		{
			switch (folderLocation)
			{
				case FolderLocations.Application:
					var appFolder = Package.Current.InstalledLocation;
					return await appFolder.OpenStreamForReadAsync(fileName);

				case FolderLocations.Storage:
					var storageFolder = ApplicationData.Current.LocalFolder;
					return await storageFolder.OpenStreamForReadAsync(fileName);

				case FolderLocations.Documents:
					var docFile = await KnownFolders.DocumentsLibrary.GetFileAsync(fileName);
					return await docFile.OpenStreamForReadAsync();

				case FolderLocations.Pictures:
					var picFile = await KnownFolders.PicturesLibrary.GetFileAsync(fileName);
					return await picFile.OpenStreamForReadAsync();

				case FolderLocations.Music:
					var musicFile = await KnownFolders.MusicLibrary.GetFileAsync(fileName);
					return await musicFile.OpenStreamForReadAsync();

				case FolderLocations.Video:
					var videoFile = await KnownFolders.VideosLibrary.GetFileAsync(fileName);
					return await videoFile.OpenStreamForReadAsync();

				default:
					Debug.LogError("Unsuported folder location: " + folderLocation);
					return null;
			}
		}

		private async void loadFileAsync(string fileName, FolderLocations folderLocation, StreamLoadedCallbackMethod streamLoadedCallback)
		{
			Stream stream = null;
			try
			{
				#if WINDOWS_PHONE
				if (folderLocation == FolderLocations.Pictures)
				{
					using (var m = new MediaLibrary())
					{
						foreach (var p in m.Pictures)
						{
							if (p.Name == fileName)
							{
								streamLoadedCallback(p.GetImage(), true);
								p.Dispose();
								return;
							}
							else
							{
								p.Dispose();
							}
						}
					}

					streamLoadedCallback(null, false);
					return;
				}
				#endif

				fileName = fileName.Replace('/', '\\');
				stream = await openStream(fileName, folderLocation);
			}
			catch (Exception e)
			{
				if (stream != null) stream.Dispose();
				Debug.LogError(e.Message);
				streamLoadedCallback(null, false);
				return;
			}

			streamLoadedCallback(stream, stream != null);
		}

		public void SaveFileDialog(Stream stream, FolderLocations folderLocation, string[] fileTypes, StreamSavedCallbackMethod streamSavedCallback)
		{
			var data = new byte[stream.Length];
			stream.Read(data, 0, data.Length);
			SaveFileDialog(data, folderLocation, fileTypes, streamSavedCallback);
		}

		public void SaveFileDialog(byte[] data, FolderLocations folderLocation, string[] fileTypes, StreamSavedCallbackMethod streamSavedCallback)
		{
			#if UNITY_METRO && !UNITY_WP_8_1
			saveFileDialogAsync(data, folderLocation, fileTypes, streamSavedCallback);
			#else
			Debug.LogError("SaveFileDialog not supported on WP8!");
			if (streamSavedCallback != null) streamSavedCallback(false);
			#endif
		}

		#if UNITY_METRO && !UNITY_WP_8_1
		private async void saveFileDialogAsync(byte[] data, FolderLocations folderLocation, string[] fileTypes, StreamSavedCallbackMethod streamSavedCallback)
		{
			await WinRTPlugin.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async delegate()
			{
				var picker = new FileSavePicker();
				if (fileTypes != null && fileTypes.Length != 0)
				{
					picker.FileTypeChoices.Add(new KeyValuePair<string,IList<string>>("Supported File Types", fileTypes));
				}

				picker.SuggestedStartLocation = getFolderType(folderLocation);
				var file = await picker.PickSaveFileAsync();
				if (file != null)
				{
					using (var fileStream = await file.OpenStreamForWriteAsync())
					{
						fileStream.Write(data, 0, data.Length);
						if (streamSavedCallback != null) streamSavedCallback(true);
					}
				}
				else
				{
					if (streamSavedCallback != null) streamSavedCallback(false);
				}
			});
		}
		#endif

		public void LoadFileDialog(FolderLocations folderLocation, int maxWidth, int maxHeight, int x, int y, int width, int height, string[] fileTypes, StreamLoadedCallbackMethod streamLoadedCallback)
		{
			if (streamLoadedCallback == null) return;
			loadFileDialogAsync(folderLocation, maxWidth, maxHeight, fileTypes, streamLoadedCallback);
		}

		#if WINDOWS_PHONE
		private string getFileExt(string fileName)
		{
			var names = fileName.Split('.');
			if (names.Length < 2) return null;
			return '.' + names[names.Length - 1];
		}

		private StreamLoadedCallbackMethod photoChooserTask_StreamLoadedCallback;
		private string[] photoChooserTask_fileTypes;
		private int photoChooserTask_maxWidth, photoChooserTask_maxHeight;
		void photoChooserTask_Completed(object sender, PhotoResult e)
		{
			var callback = photoChooserTask_StreamLoadedCallback;
			var fileTypes = photoChooserTask_fileTypes;
			int maxWidth = photoChooserTask_maxWidth, maxHeight = photoChooserTask_maxHeight;
			photoChooserTask_StreamLoadedCallback = null;
			photoChooserTask_fileTypes = null;
			photoChooser = null;

			if (e.TaskResult != TaskResult.OK)
			{
				if (callback != null) callback(null, false);
				return;
			}

			string fileExt = getFileExt(e.OriginalFileName).ToLower();
			foreach (var type in fileTypes)
			{
				if (getFileExt(type).ToLower() == fileExt)
				{
					if (callback != null && e.TaskResult == TaskResult.OK)
					{
						var stream = e.ChosenPhoto;
						if (maxWidth == 0 || maxHeight == 0)
						{
							callback(stream, true);
						}
						else
						{
							var scaledStream = resizeImageStream(stream, maxWidth, maxHeight);
							callback(scaledStream, true);
						}
					}
					else if (callback != null)
					{
						callback(null, false);
					}

					return;
				}
			}

			if (callback != null) callback(null, false);
		}
		#endif

		#if UNITY_WP_8_1
		private static FileOpenPicker LoadFileDialog_picker;
		private static StreamLoadedCallbackMethod LoadFileDialog_streamLoadedCallback;
		private static int LoadFileDialog_maxWidth, LoadFileDialog_maxHeight;
		internal static async void loadFileDialog_Callback(Windows.ApplicationModel.Activation.IFileOpenPickerContinuationEventArgs data)
		{
			if (data == null)
			{
				LoadFileDialog_streamLoadedCallback(null, false);
				return;
			}

			var files = data.Files;
			if (files != null && files.Count >= 1)
			{
				var file = files[0];
				Stream stream = null;
				if (LoadFileDialog_maxWidth == 0 || LoadFileDialog_maxHeight == 0)
				{
					stream = await file.OpenStreamForReadAsync();
					LoadFileDialog_streamLoadedCallback(stream, true);
				}
				else
				{
					using (var tempStream = await file.OpenStreamForReadAsync())
					{
						stream = await resizeImageStream(tempStream.AsRandomAccessStream(), LoadFileDialog_maxWidth, LoadFileDialog_maxHeight);
					}

					LoadFileDialog_streamLoadedCallback(stream, true);
				}
			}
			else
			{
				LoadFileDialog_streamLoadedCallback(null, false);
			}

			LoadFileDialog_picker = null;
		}
		#endif

		#if WINDOWS_PHONE
		private void loadFileDialogAsync(FolderLocations folderLocation, int maxWidth, int maxHeight, string[] fileTypes, StreamLoadedCallbackMethod streamLoadedCallback)
		#else
		private async void loadFileDialogAsync(FolderLocations folderLocation, int maxWidth, int maxHeight, string[] fileTypes, StreamLoadedCallbackMethod streamLoadedCallback)
		#endif
		{
			#if WINDOWS_PHONE
			WinRTPlugin.Dispatcher.BeginInvoke(async delegate()
			#elif UNITY_WP_8_1
			await WinRTPlugin.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate()
			#else
			await WinRTPlugin.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async delegate()
			#endif
			{
				Stream stream = null;
				try
				{
					#if WINDOWS_PHONE
					if (folderLocation == FolderLocations.Pictures)
					{
						photoChooser = new PhotoChooserTask();
						photoChooser.ShowCamera = false;
						photoChooser.Completed += photoChooserTask_Completed;
						photoChooserTask_StreamLoadedCallback = streamLoadedCallback;
						photoChooserTask_fileTypes = fileTypes;
						photoChooserTask_maxWidth = maxWidth;
						photoChooserTask_maxHeight = maxHeight;
						photoChooser.Show();
						return;
					}
					#endif

					var picker = new FileOpenPicker();
					if (fileTypes != null)
					{
						foreach (var fileType in fileTypes) picker.FileTypeFilter.Add(fileType);
					}

					picker.SuggestedStartLocation = getFolderType(folderLocation);
					#if UNITY_WP_8_1
					LoadFileDialog_picker = picker;
					LoadFileDialog_streamLoadedCallback = streamLoadedCallback;
					LoadFileDialog_maxWidth = maxWidth;
					LoadFileDialog_maxHeight = maxHeight;
					picker.PickSingleFileAndContinue();
					#else
					var file = await picker.PickSingleFileAsync();
					if (file != null)
					{
						if (maxWidth == 0 || maxHeight == 0)
						{
							stream = await file.OpenStreamForReadAsync();
							streamLoadedCallback(stream, true);
						}
						else
						{
							#if UNITY_METRO
							using (var tempStream = await file.OpenStreamForReadAsync())
							{
								#if UNITY_METRO_8_0
								using (var outputStream = new MemoryStream().AsOutputStream())
								{
									await RandomAccessStream.CopyAsync(tempStream.AsInputStream(), outputStream);
									stream = await resizeImageStream((IRandomAccessStream)outputStream, maxWidth, maxHeight);
								}
								#else
								stream = await resizeImageStream(tempStream.AsRandomAccessStream(), maxWidth, maxHeight);
								#endif
							}

							streamLoadedCallback(stream, true);
							#endif
						}
					}
					else
					{
						streamLoadedCallback(null, false);
					}
					#endif
				}
				catch (Exception e)
				{
					if (stream != null) stream.Dispose();
					Debug.LogError(e.Message);
					streamLoadedCallback(null, false);
				}
			});
		}

		private PickerLocationId getFolderType(FolderLocations folderLocation)
		{
			PickerLocationId folder = PickerLocationId.Desktop;
			switch (folderLocation)
			{
				case FolderLocations.Documents: folder = PickerLocationId.DocumentsLibrary; break;
				case FolderLocations.Pictures: folder = PickerLocationId.PicturesLibrary; break;
				case FolderLocations.Music: folder = PickerLocationId.MusicLibrary; break;
				case FolderLocations.Video: folder = PickerLocationId.VideosLibrary; break;
				default: Debug.LogError("Unsuported folder location: " + folderLocation); break;
			}

			return folder;
		}

		#if WINDOWS_PHONE
		private static Stream resizeImageStream(Stream imageStream, int maxWidth, int maxHeight)
		#else
		private static async Task<Stream> resizeImageStream(IRandomAccessStream imageStream, int maxWidth, int maxHeight)
		#endif
		{
			#if WINDOWS_PHONE
			var image = new BitmapImage();
			image.CreateOptions = BitmapCreateOptions.None;
			image.SetSource(imageStream);

			var scaledStream = new MemoryStream();
			var newImage = new WriteableBitmap(image);
			Vector2 newSize;
			if (image.PixelWidth > maxWidth || image.PixelHeight > maxHeight) newSize = Reign.MathUtilities.FitInViewIfLarger(newImage.PixelWidth, newImage.PixelHeight, maxWidth, maxHeight);
			else newSize = new Vector2(image.PixelWidth, image.PixelHeight);
			newImage.SaveJpeg(scaledStream, (int)newSize.x, (int)newSize.y, 0, 95);
			scaledStream.Position = 0;

			return scaledStream;
			#else
			var decoder = await BitmapDecoder.CreateAsync(imageStream);
								
			var newStream = new InMemoryRandomAccessStream();
			var encoder = await BitmapEncoder.CreateForTranscodingAsync(newStream, decoder);
			Vector2 newSize;
			if (decoder.PixelWidth > maxWidth || decoder.PixelHeight > maxHeight) newSize = Reign.MathUtilities.FitInViewIfLarger(decoder.PixelWidth, decoder.PixelHeight, maxWidth, maxHeight);
			else newSize = new Vector2(decoder.PixelWidth, decoder.PixelHeight);
			encoder.BitmapTransform.ScaledWidth = (uint)newSize.x;
			encoder.BitmapTransform.ScaledHeight = (uint)newSize.y;
			await encoder.FlushAsync();

			return newStream.AsStream();
			#endif
		}

		public void LoadCameraPicker(CameraQuality quality, int maxWidth, int maxHeight, StreamLoadedCallbackMethod streamLoadedCallback)
		{
			if (streamLoadedCallback == null) streamLoadedCallback(null, false);
			else loadCameraPickerAsync(quality, maxWidth, maxHeight, streamLoadedCallback);
		}

		#if WINDOWS_PHONE || UNITY_WP_8_1
		private void loadCameraPickerAsync(CameraQuality quality, int maxWidth, int maxHeight, StreamLoadedCallbackMethod streamLoadedCallback)
		#else
		private async void loadCameraPickerAsync(CameraQuality quality, int maxWidth, int maxHeight, StreamLoadedCallbackMethod streamLoadedCallback)
		#endif
		{
			#if WINDOWS_PHONE
			WinRTPlugin.Dispatcher.BeginInvoke(delegate()
			{
				loadCameraPicker_streamLoadedCallback = streamLoadedCallback;
				loadCameraPicker_maxWidth = maxWidth;
				loadCameraPicker_maxHeight = maxHeight;

				var cameraCaptureTask = new CameraCaptureTask();
				cameraCaptureTask.Completed += new EventHandler<PhotoResult>(cameraCaptureTask_Completed);
				cameraCaptureTask.Show();
			});
			#elif UNITY_WP_8_1
			loadFileDialogAsync(FolderLocations.Pictures, maxWidth, maxHeight, new string[]{".jpg"}, streamLoadedCallback);
			#else
			await WinRTPlugin.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async delegate()
			{
				try
				{
					var cameraUI = new CameraCaptureUI();
					cameraUI.PhotoSettings.AllowCropping = false;
					switch (quality)
					{
						case CameraQuality.Low: cameraUI.PhotoSettings.MaxResolution = CameraCaptureUIMaxPhotoResolution.SmallVga; break;
						case CameraQuality.Med: cameraUI.PhotoSettings.MaxResolution = CameraCaptureUIMaxPhotoResolution.MediumXga; break;
						case CameraQuality.High: cameraUI.PhotoSettings.MaxResolution = CameraCaptureUIMaxPhotoResolution.HighestAvailable; break;
					}

					var capturedMedia = await cameraUI.CaptureFileAsync(CameraCaptureUIMode.Photo);
					if (capturedMedia != null)
					{
						using (var cameraStream = await capturedMedia.OpenAsync(FileAccessMode.Read))
						{
							if (maxWidth == 0 || maxHeight == 0)
							{
								streamLoadedCallback(cameraStream.AsStream(), true);
							}
							else
							{
								var stream = await resizeImageStream(cameraStream, maxWidth, maxHeight);
								streamLoadedCallback(stream, true);
							}
						}
					}
					else
					{
						streamLoadedCallback(null, false);
					}
				}
				catch (Exception e)
				{
					Debug.LogError(e.Message);
					streamLoadedCallback(null, false);
				}
			});
			#endif
		}

		#if WINDOWS_PHONE
		StreamLoadedCallbackMethod loadCameraPicker_streamLoadedCallback;
		int loadCameraPicker_maxWidth, loadCameraPicker_maxHeight;
		private void cameraCaptureTask_Completed(object sender, PhotoResult e)
		{
			if (e.TaskResult == TaskResult.OK)
			{
				if (loadCameraPicker_maxWidth == 0 || loadCameraPicker_maxHeight == 0)
				{
					loadCameraPicker_streamLoadedCallback(e.ChosenPhoto, true);
				}
				else
				{
					var stream = resizeImageStream(e.ChosenPhoto, loadCameraPicker_maxWidth, loadCameraPicker_maxHeight);
					loadCameraPicker_streamLoadedCallback(stream, true);
				}
			}
		}
		#endif
	}
}
#elif UNITY_WINRT
using System.IO;

namespace Reign.Plugin
{
	public class StreamPlugin_WinRT : IStreamPlugin
	{
		public IStreamPlugin Native;
		public delegate void InitNativeMethod(StreamPlugin_WinRT plugin);
		public static InitNativeMethod InitNative;

		public StreamPlugin_WinRT()
		{
			InitNative(this);
		}

		public void FileExists(string fileName, FolderLocations folderLocation, StreamExistsCallbackMethod callback)
		{
			Native.FileExists(fileName, folderLocation, callback);
		}

		public void DeleteFile(string fileName, FolderLocations folderLocation, StreamDeleteCallbackMethod callback)
		{
			Native.DeleteFile(fileName, folderLocation, callback);
		}

		public void SaveFile(string fileName, Stream stream, FolderLocations folderLocation, StreamSavedCallbackMethod streamSavedCallback)
		{
			Native.SaveFile(fileName, stream, folderLocation, streamSavedCallback);
		}

		public void SaveFile(string fileName, byte[] data, FolderLocations folderLocation, StreamSavedCallbackMethod streamSavedCallback)
		{
			Native.SaveFile(fileName, data, folderLocation, streamSavedCallback);
		}

		public void LoadFile(string fileName, FolderLocations folderLocation, StreamLoadedCallbackMethod streamLoadedCallback)
		{
			Native.LoadFile(fileName, folderLocation, streamLoadedCallback);
		}

		public void SaveFileDialog(Stream stream, FolderLocations folderLocation, string[] fileTypes, StreamSavedCallbackMethod streamSavedCallback)
		{
			Native.SaveFileDialog(stream, folderLocation, fileTypes, streamSavedCallback);
		}

		public void SaveFileDialog(byte[] data, FolderLocations folderLocation, string[] fileTypes, StreamSavedCallbackMethod streamSavedCallback)
		{
			Native.SaveFileDialog(data, folderLocation, fileTypes, streamSavedCallback);
		}

		public void LoadFileDialog(FolderLocations folderLocation, int maxWidth, int maxHeight, int x, int y, int width, int height, string[] fileTypes, StreamLoadedCallbackMethod streamLoadedCallback)
		{
			Native.LoadFileDialog(folderLocation, maxWidth, maxHeight, x, y, width, height, fileTypes, streamLoadedCallback);
		}

		public void LoadCameraPicker(CameraQuality quality, int maxWidth, int maxHeight, StreamLoadedCallbackMethod streamLoadedCallback)
		{
			Native.LoadCameraPicker(quality, maxWidth, maxHeight, streamLoadedCallback);
		}

		public void Update()
		{
			Native.Update();
		}
	}
}
#endif