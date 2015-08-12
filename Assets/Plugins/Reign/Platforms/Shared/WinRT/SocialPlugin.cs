#if REIGN_POSTBUILD
using System;
using System.Collections.Generic;

#if WINDOWS_PHONE
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Media.PhoneExtensions;
#endif

#if UNITY_METRO || UNITY_WP_8_1
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Core;
#endif

namespace Reign.Plugin
{
	public class SocialPlugin_Native : ISocialPlugin
	{
		#if UNITY_METRO || UNITY_WP_8_1
		private bool shareImage;
		private string shareText, shareTitle, shareDesc, shareDataFilename;
		#endif

		public void Init(SocialDesc desc)
		{
			// do nothing...
		}

		public void Share(byte[] data, string dataFilename, string text, string title, string desc, SocialShareDataTypes type)
		{
			#if WINDOWS_PHONE
			if (data != null)
			{
				string filename;
				using (var m = new MediaLibrary())
				using (var image = m.SavePicture(dataFilename + (type == SocialShareDataTypes.Image_PNG ? ".png" : ".jpg"), data))
				{
					filename = MediaLibraryExtensions.GetPath(image);
				}

				var shareTask = new ShareMediaTask();
				shareTask.FilePath = filename;
				shareTask.Show();
			}
			else if (!string.IsNullOrEmpty(text))
			{
				var shareTask = new ShareStatusTask();
				shareTask.Status = text;
				shareTask.Show();
			}
			#else
			shareText = text;
			shareTitle = title;
			shareDesc = desc;
			if (data != null)
			{
				shareImage = true;
				shareDataFilename = dataFilename + (type == SocialShareDataTypes.Image_PNG ? ".png" : ".jpg");
				StreamManager.SaveFile(shareDataFilename, data, FolderLocations.Storage, imageSavedCallback);
			}
			else
			{
				imageSavedCallback(true);
			}
			#endif
		}

		public void Share(byte[] data, string dataFilename, string text, string title, string desc, int x, int y, int width, int height, SocialShareDataTypes type)
		{
			Share(data, dataFilename, text, title, desc, type);
		}

		#if UNITY_METRO || UNITY_WP_8_1
		private async void imageSavedCallback(bool succeeded)
		{
			await WinRTPlugin.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate()
			{
				var dataTransferManager = DataTransferManager.GetForCurrentView();
				dataTransferManager.DataRequested -= shareStorageItemsHandler;
				dataTransferManager.DataRequested += shareStorageItemsHandler;
				DataTransferManager.ShowShareUI();
			});
		}

		private async void shareStorageItemsHandler(DataTransferManager sender, DataRequestedEventArgs e)
		{
			DataRequest request = e.Request;
			request.Data.Properties.Title = shareTitle;
			request.Data.Properties.Description = shareDesc;
			DataRequestDeferral deferral = request.GetDeferral();
			try
			{
				if (!string.IsNullOrEmpty(shareText)) request.Data.SetText(shareText);  
				if (shareImage)
				{
					StorageFile shareFile = await ApplicationData.Current.LocalFolder.GetFileAsync(shareDataFilename);
					var storageItems = new List<IStorageItem>();
					storageItems.Add(shareFile);
					request.Data.SetStorageItems(storageItems);
				}   
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogError(ex.Message);
			}
			finally
			{
				deferral.Complete();
			}
		}
		#endif
	}
}
#elif UNITY_WINRT
using UnityEngine;
using System.Collections;

namespace Reign.Plugin
{
	public class SocialPlugin_WinRT : ISocialPlugin
	{
		public ISocialPlugin Native;
		public delegate void InitNativeMethod(SocialPlugin_WinRT plugin);
		public static InitNativeMethod InitNative;

		public SocialPlugin_WinRT()
		{
			InitNative(this);
		}

		public void Init(SocialDesc desc)
		{
			Native.Init(desc);
		}

		public void Share(byte[] data, string dataFilename, string text, string title, string desc, SocialShareDataTypes type)
		{
			Native.Share(data, dataFilename, text, title, desc, type);
		}

		public void Share(byte[] data, string dataFilename, string text, string title, string desc, int x, int y, int width, int height, SocialShareDataTypes type)
		{
			Native.Share(data, dataFilename, text, title, desc, x, y, width, height, type);
		}
	}
}
#endif