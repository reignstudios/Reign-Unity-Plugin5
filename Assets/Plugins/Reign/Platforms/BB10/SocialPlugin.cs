#if UNITY_BLACKBERRY
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.InteropServices;
using System.IO;

namespace Reign.Plugin
{
	public class SocialPlugin_BB10 : ISocialPlugin
	{
		private IntPtr invoke;
		private SocialDesc desc;

		private string shareText, shareDataFilename;
		private byte[] shareData;
		private SocialShareDataTypes shareType;

		public void Init(SocialDesc desc)
		{
			this.desc = desc;
			desc.BB10_CloseButton.onClick.AddListener(closeButtonClicked);
			desc.BB10_ShareSelectorBBM.onClick.AddListener(bbmButtonClicked);
			desc.BB10_ShareSelectorFacebook.onClick.AddListener(facebookButtonClicked);
			desc.BB10_ShareSelectorTwitter.onClick.AddListener(twitterButtonClicked);
		}

		private void closeButtonClicked()
		{
			this.desc.BB10_ShareSelectorUI.SetActive(false);
		}

		private void bbmButtonClicked()
		{
			this.desc.BB10_ShareSelectorUI.SetActive(false);
			share(0);
		}

		private void facebookButtonClicked()
		{
			this.desc.BB10_ShareSelectorUI.SetActive(false);
			share(1);
		}

		private void twitterButtonClicked()
		{
			this.desc.BB10_ShareSelectorUI.SetActive(false);
			share(2);
		}

		public void Share(byte[] data, string dataFilename, string text, string title, string desc, SocialShareDataTypes type)
		{
			Share(data, dataFilename, text, title, desc, 0, 0, 10, 10, type);
		}

		public void Share(byte[] data, string dataFilename, string text, string title, string desc, int x, int y, int width, int height, SocialShareDataTypes type)
		{
			this.desc.BB10_ShareSelectorTitle.text = title;
			shareDataFilename = dataFilename;
			shareText = text;
			shareData = data;
			shareType = type;
			this.desc.BB10_ShareSelectorUI.SetActive(true);
		}

		private void share(int appType)
		{
			// store temp data so the GC can collect after done
			var data = shareData;
			shareData = null;

			// check data type is valid
			if (data != null && shareType != SocialShareDataTypes.Image_PNG && shareType != SocialShareDataTypes.Image_JPG)
			{
				Debug.LogError("Unusported data Share type: " + shareType);
				return;
			}

			// share
			if (Common.navigator_invoke_invocation_create(ref invoke) != 0) return;
			if (appType == 0 && Common.navigator_invoke_invocation_set_target(invoke, "sys.bbm.sharehandler") != 0)
			{
				Common.navigator_invoke_invocation_destroy(invoke);
				return;
			}

			if (appType == 1 && Common.navigator_invoke_invocation_set_target(invoke, "Facebook") != 0)
			{
				Common.navigator_invoke_invocation_destroy(invoke);
				return;
			}

			if (appType == 2 && Common.navigator_invoke_invocation_set_target(invoke, "Twitter") != 0)
			{
				Common.navigator_invoke_invocation_destroy(invoke);
				return;
			}

			if (Common.navigator_invoke_invocation_set_action(invoke, "bb.action.SHARE") != 0)
			{
				Common.navigator_invoke_invocation_destroy(invoke);
				return;
			}

			IntPtr dataValue = IntPtr.Zero;
			if (data != null)
			{
				if (Common.navigator_invoke_invocation_set_type(invoke, shareType == SocialShareDataTypes.Image_PNG ? "image/png" : "image/jpg") != 0)
				{
					Common.navigator_invoke_invocation_destroy(invoke);
					return;
				}

				//string filename = "data/"+shareDataFilename + (shareType == SocialShareDataTypes.Image_PNG ? ".png" : ".jpg");// Other apps can't seem to read data from this location
				string filename = "/accounts/1000/shared/photos/"+shareDataFilename + (shareType == SocialShareDataTypes.Image_PNG ? ".png" : ".jpg");
				dataValue = Marshal.StringToHGlobalAnsi(filename);
				try
				{
					using (var stream = new FileStream(filename, FileMode.Create, FileAccess.Write))
					{
						stream.Write(data, 0, data.Length);
					}
				}
				catch (Exception e)
				{
					Debug.LogError("Failed to save share image: " + e.Message);
					return;
				}

				if (Common.navigator_invoke_invocation_set_data(invoke, dataValue, filename.Length) != 0)
				{
					Common.navigator_invoke_invocation_destroy(invoke);
					return;
				}
			}
			else if (!string.IsNullOrEmpty(shareText))
			{
				if (Common.navigator_invoke_invocation_set_type(invoke, "text/plain") != 0)
				{
					Common.navigator_invoke_invocation_destroy(invoke);
					return;
				}

				dataValue = Marshal.StringToHGlobalAnsi(shareText);
				if (Common.navigator_invoke_invocation_set_data(invoke, dataValue, shareText.Length) != 0)
				{
					Common.navigator_invoke_invocation_destroy(invoke);
					return;
				}
			}

			if (Common.navigator_invoke_invocation_send(invoke) != 0)
			{
				Common.navigator_invoke_invocation_destroy(invoke);
				return;
			}

			Common.navigator_invoke_invocation_destroy(invoke);
			if (dataValue != IntPtr.Zero) Marshal.FreeHGlobal(dataValue);
		}
	}
}
#endif