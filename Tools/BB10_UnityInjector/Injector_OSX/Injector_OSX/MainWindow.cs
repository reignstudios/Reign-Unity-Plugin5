using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using Injector;

namespace Injector_OSX
{
	public partial class MainWindow : MonoMac.AppKit.NSWindow
	{
		#region Constructors
		// Called when created from unmanaged code
		public MainWindow (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public MainWindow (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		
		// Shared initialization code
		void Initialize ()
		{
			StyleMask = NSWindowStyle.Titled | NSWindowStyle.Miniaturizable | NSWindowStyle.Closable;
			Center();
		}
		#endregion
		
		private string checkNull(string value)
		{
			if (value == null) return "";
			return value;
		}
		
		public void Create()
		{
			// load reign image
			string path = NSBundle.MainBundle.PathForResource("Images/Logo", "png", "", "Images/");
			var image = new NSImage(path);
			imageView.Image = image;
			
			// set UI values
			InjectorCore.FindUnityPath();
			InjectorCore.LoadPaths();
			unityPath.StringValue = checkNull(InjectorCore.UnityPath);
			barFileName.StringValue = checkNull(InjectorCore.BarFileName);
			scoreloopPath.StringValue = checkNull(InjectorCore.ScoreloopBundlePath);
			keyName.StringValue = checkNull(InjectorCore.KeyPassword);
			signBarFile.State = InjectorCore.SignBarFile ? NSCellStateValue.On : NSCellStateValue.Off;
			phoneIP.StringValue = checkNull(InjectorCore.PhoneIP);
			phoneLockPass.StringValue = checkNull(InjectorCore.PhonePass);

			unityClassicMode.Activated += unityClassicMode_Click;
			
			unityPath.Changed += unityPath_Changed;
			barFileName.Changed += barFileName_Changed;
			scoreloopPath.Changed += scoreloopPath_Changed;
			keyName.Changed += keyName_Changed;
			phoneIP.Changed += phoneIP_Changed;
			phoneLockPass.Changed += phoneLockPass_Changed;
			
			// get button events
			applyButton.Activated += applyButton_Click;
			selectBarFile.Activated += selectBarFile_Click;
			selectScoreloopData.Activated += selectScoreloopData_Click;
			signBarFile.Activated += signBarFile_Click;
			uploadButton.Activated += uploadButton_Click;
		}
		
		private void MessageBox(string message, string title)
		{
			var alert = new NSAlert();
			alert.AddButton ("OK");
			alert.MessageText = title;
			alert.InformativeText = message;
			
			alert.BeginSheet(this, delegate
			{
			    alert.Dispose();
			});
		}

		private void unityClassicMode_Click(object sender, EventArgs args)
		{
			InjectorCore.UseClassicMode = unityClassicMode.State == NSCellStateValue.On;
		}
		
		private void applyButton_Click(object sender, EventArgs args)
		{
			string error = InjectorCore.ProcessBarFile();
			if (error != null) MessageBox(error, "Error");
			else MessageBox("Scoreloop inject complete, but remember to check log.txt for any packaging errors.", "Success");
		}
		
		private void selectBarFile_Click(object sender, EventArgs args)
		{
			var openPanel = new NSOpenPanel();
			openPanel.ReleasedWhenClosed = true;
			openPanel.Prompt = "Select bar file";
			
			var result = openPanel.RunModal();
			if (result == 1)
			{
				barFileName.StringValue = openPanel.Url.Path;
				InjectorCore.BarFileName = openPanel.Url.Path;
			}
		}
		
		private void selectScoreloopData_Click(object sender, EventArgs args)
		{
			var openPanel = new NSOpenPanel();
			openPanel.ReleasedWhenClosed = true;
			openPanel.Prompt = "Select bundle file";
			
			var result = openPanel.RunModal();
			if (result == 1)
			{
				scoreloopPath.StringValue = openPanel.Url.Path;
				InjectorCore.ScoreloopBundlePath = openPanel.Url.Path;
			}
		}
		
		private void signBarFile_Click(object sender, EventArgs args)
		{
			InjectorCore.SignBarFile = signBarFile.State == NSCellStateValue.On;
		}
		
		private void unityPath_Changed(object sender, EventArgs e)
		{
			InjectorCore.UnityPath = unityPath.StringValue;
		}
		
		private void barFileName_Changed(object sender, EventArgs e)
		{
			InjectorCore.BarFileName = barFileName.StringValue;
		}
		
		private void scoreloopPath_Changed(object sender, EventArgs e)
		{
			InjectorCore.ScoreloopBundlePath = scoreloopPath.StringValue;
		}
		
		private void keyName_Changed(object sender, EventArgs e)
		{
			InjectorCore.KeyPassword = keyName.StringValue;
		}
		
		private void uploadButton_Click(object sender, EventArgs args)
		{
			var openPanel = new NSOpenPanel();
			openPanel.ReleasedWhenClosed = true;
			openPanel.Prompt = "Open bar file";
			
			var result = openPanel.RunModal();
			if (result == 1)
			{
				string error = InjectorCore.Upload(openPanel.Url.Path);
				if (error != null) MessageBox(error, "Error");
				else MessageBox("Upload complete, but remember to check log.txt for any upload errors.", "Success");
			}
		}
		
		private void phoneIP_Changed(object sender, EventArgs e)
		{
			InjectorCore.PhoneIP = phoneIP.StringValue;
		}
		
		private void phoneLockPass_Changed(object sender, EventArgs e)
		{
			InjectorCore.PhonePass = phoneLockPass.StringValue;
		}
	}
}

