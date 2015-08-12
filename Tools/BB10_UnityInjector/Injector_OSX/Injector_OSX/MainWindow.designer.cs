// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;
using System.CodeDom.Compiler;

namespace Injector_OSX
{
	[Register ("MainWindow")]
	partial class MainWindow
	{
		[Outlet]
		MonoMac.AppKit.NSButton applyButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField barFileName { get; set; }

		[Outlet]
		MonoMac.AppKit.NSImageView imageView { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField keyName { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField phoneIP { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField phoneLockPass { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField scoreloopPath { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton selectBarFile { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton selectScoreloopData { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton selectUnity { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton signBarFile { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton unityClassicMode { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField unityPath { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton uploadButton { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (unityClassicMode != null) {
				unityClassicMode.Dispose ();
				unityClassicMode = null;
			}

			if (applyButton != null) {
				applyButton.Dispose ();
				applyButton = null;
			}

			if (barFileName != null) {
				barFileName.Dispose ();
				barFileName = null;
			}

			if (imageView != null) {
				imageView.Dispose ();
				imageView = null;
			}

			if (keyName != null) {
				keyName.Dispose ();
				keyName = null;
			}

			if (phoneIP != null) {
				phoneIP.Dispose ();
				phoneIP = null;
			}

			if (phoneLockPass != null) {
				phoneLockPass.Dispose ();
				phoneLockPass = null;
			}

			if (scoreloopPath != null) {
				scoreloopPath.Dispose ();
				scoreloopPath = null;
			}

			if (selectBarFile != null) {
				selectBarFile.Dispose ();
				selectBarFile = null;
			}

			if (selectScoreloopData != null) {
				selectScoreloopData.Dispose ();
				selectScoreloopData = null;
			}

			if (selectUnity != null) {
				selectUnity.Dispose ();
				selectUnity = null;
			}

			if (signBarFile != null) {
				signBarFile.Dispose ();
				signBarFile = null;
			}

			if (unityPath != null) {
				unityPath.Dispose ();
				unityPath = null;
			}

			if (uploadButton != null) {
				uploadButton.Dispose ();
				uploadButton = null;
			}
		}
	}

	[Register ("MainWindowController")]
	partial class MainWindowController
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
