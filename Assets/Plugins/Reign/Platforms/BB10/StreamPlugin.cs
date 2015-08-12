#if UNITY_BLACKBERRY
using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Reign.Plugin
{
	/*struct img_decode_callouts_t
	{
		public IntPtr choose_format_f;
		public IntPtr setup_f;
		public IntPtr abort_f;
		public IntPtr scanline_f;
		public IntPtr set_palette_f;
		public IntPtr set_transparency_f;
		public IntPtr frame_f;
		public IntPtr set_value_f;
		public uint data;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct img_t
	{
		public IntPtr data;
		public uint stride;
		public uint w, h;
		public uint format;// enum type
		public uint npalette;
		public IntPtr palette;
		public uint flags;
		public uint rgb32;
		public uint quality;    
	}*/

	public class StreamPlugin_BB10 : StreamPluginBase
	{
		/*[DllImport("libimg", EntryPoint="img_load_file")]
		private static unsafe extern int img_load_file(IntPtr ilib, string path, img_decode_callouts_t* callouts, img_t* img);

		[DllImport("libimg", EntryPoint="img_resize_fs")]
		private static unsafe extern int img_resize_fs(img_t* src, img_t* dst);

		[DllImport("libimg", EntryPoint="img_lib_attach")]
		private static unsafe extern int img_lib_attach(IntPtr* ilib);

		[DllImport("libimg", EntryPoint="img_lib_detach")]
		private static unsafe extern void img_lib_detach(IntPtr ilib);*/

		private IntPtr invoke;
		private const int NAVIGATOR_INVOKE_TARGET_RESULT = 0x13;
		private const int NAVIGATOR_CHILD_CARD_CLOSED = 0x21;

		public override void SaveFile(string fileName, byte[] data, FolderLocations folderLocation, StreamSavedCallbackMethod steamSavedCallback)
		{
			if (folderLocation == FolderLocations.Pictures)
			{
				using (var file = new FileStream("/accounts/1000/shared/photos/"+fileName, FileMode.Create, FileAccess.Write))
				{
					file.Write(data, 0, data.Length);
				}
					
				if(steamSavedCallback != null) steamSavedCallback(true);
			}
			else if (folderLocation != FolderLocations.Storage)
			{
				Debug.LogError("Save file in folder location: " + folderLocation + " is not supported.");
				if (steamSavedCallback != null) steamSavedCallback(false);
			}
			else
			{
				base.SaveFile(fileName, data, folderLocation, steamSavedCallback);
			}
		}

		public override void LoadFile(string fileName, FolderLocations folderLocation, StreamLoadedCallbackMethod streamLoadedCallback)
		{
			if (folderLocation != FolderLocations.Storage)
			{
				Debug.LogError("Load file in folder location: " + folderLocation + " is not supported.");
				streamLoadedCallback(null, false);
			}
			else
			{
				base.LoadFile(fileName, folderLocation, streamLoadedCallback);
			}
		}

		private string[] addUppers(string[] fileTypes)
		{
			var items = new System.Collections.Generic.List<string>();
			foreach (var type in fileTypes)
			{
				items.Add(type);
				items.Add(type.ToUpper());
			}

			return items.ToArray();
		}

		public override void LoadFileDialog(FolderLocations folderLocation, int maxWidth, int maxHeight, int x, int y, int width, int height, string[] fileTypes, StreamLoadedCallbackMethod streamLoadedCallback)
		{
			if (folderLocation != FolderLocations.Pictures)
			{
				Debug.LogError("LoadFileDialog not supported for folder location: " + folderLocation + " on this Platform yet.");
				streamLoadedCallback(null, false);
			}
			else
			{
				if (Common.navigator_invoke_invocation_create(ref invoke) != 0) return;
				if (Common.navigator_invoke_invocation_set_target(invoke, "sys.filepicker.target") != 0)// sys.filesaver.target << use for file save dialog
				{
					Common.navigator_invoke_invocation_destroy(invoke);
					return;
				}
			
				if (Common.navigator_invoke_invocation_set_action(invoke, "bb.action.OPEN") != 0)
				{
					Common.navigator_invoke_invocation_destroy(invoke);
					return;
				}
			
				if (Common.navigator_invoke_invocation_set_type(invoke, "application/vnd.blackberry.file_picker") != 0)
				{
					Common.navigator_invoke_invocation_destroy(invoke);
					return;
				}
			
				if (Common.navigator_invoke_invocation_send(invoke) != 0)
				{
					Common.navigator_invoke_invocation_destroy(invoke);
					return;
				}

				var dataPathPatterns = new string[]
				{
					@"file\://(/accounts/1000/shared/photos/)([\w|\.|\-]*)",
					@"file\://(/accounts/1000/shared/camera/)([\w|\.|\-]*)",
					@"file\://(/accounts/1000/shared/downloads/)([\w|\.|\-]*)"
				};
				waitAndProcessImage(addUppers(fileTypes), dataPathPatterns, maxWidth, maxHeight, streamLoadedCallback);
			
				Common.navigator_invoke_invocation_destroy(invoke);
			}
		}

		public override void LoadCameraPicker (CameraQuality quality, int maxWidth, int maxHeight, StreamLoadedCallbackMethod streamLoadedCallback)
		{
			if (Common.navigator_invoke_invocation_create(ref invoke) != 0) return;
			if (Common.navigator_invoke_invocation_set_target(invoke, "sys.camera.card") != 0)
			{
				Common.navigator_invoke_invocation_destroy(invoke);
				return;
			}
		
			if (Common.navigator_invoke_invocation_set_action(invoke, "bb.action.CAPTURE") != 0)
			{
				Common.navigator_invoke_invocation_destroy(invoke);
				return;
			}
		
			if (Common.navigator_invoke_invocation_set_type(invoke, "image/jpeg") != 0)
			{
				Common.navigator_invoke_invocation_destroy(invoke);
				return;
			}

			string name = "photo";
			IntPtr namePtr = Marshal.StringToHGlobalAnsi(name);
			if (Common.navigator_invoke_invocation_set_data(invoke, namePtr, name.Length) != 0)
			{
				Common.navigator_invoke_invocation_destroy(invoke);
				return;
			}
		
			if (Common.navigator_invoke_invocation_send(invoke) != 0)
			{
				Common.navigator_invoke_invocation_destroy(invoke);
				return;
			}

			var fileTypes = new string[]
			{
				".jpg",
				"jpeg",
				".png"
			};
			var dataPathPatterns = new string[]
			{
				@"(/accounts/1000/shared/camera/)([\w|\.|\-]*)",
				@"(/accounts/1000/shared/photos/)([\w|\.|\-]*)"
			};
			waitAndProcessImage(addUppers(fileTypes), dataPathPatterns, maxWidth, maxHeight, streamLoadedCallback);

			Common.navigator_invoke_invocation_destroy(invoke);
			Marshal.FreeHGlobal(namePtr);
		}

		private static void waitAndProcessImage(string[] fileTypes, string[] dataPathPatterns, int maxWidth, int maxHeight, StreamLoadedCallbackMethod streamLoadedCallback)
		{
			// wait for event
			while (true)
			{
				IntPtr _event = IntPtr.Zero;
				Common.bps_get_event(ref _event, -1);// wait here for next event
				if (_event != IntPtr.Zero)
				{
					if (Common.bps_event_get_code(_event) == NAVIGATOR_CHILD_CARD_CLOSED)
					{
						IntPtr reasonPtr = Common.navigator_event_get_card_closed_reason(_event);
						string reason = Marshal.PtrToStringAnsi(reasonPtr);
						Debug.Log("reason: " + reason);
						if (reason == "save")//save - cancel
						{
							IntPtr dataPathPtr = Common.navigator_event_get_card_closed_data(_event);
							string dataPath = Marshal.PtrToStringAnsi(dataPathPtr);
							Debug.Log("Loading file from dataPath: " + dataPath);
						
							try
							{
								System.Text.RegularExpressions.MatchCollection matches = null;
								foreach (var pattern in dataPathPatterns)
								{
									matches = System.Text.RegularExpressions.Regex.Matches(dataPath, pattern);
									if (matches.Count != 0) break;
								}

								if (matches != null)
								foreach (System.Text.RegularExpressions.Match match in matches)
								{
									if (match.Groups.Count == 3)
									{
										string path = match.Groups[1].Value;
										string fileName = match.Groups[2].Value;
									
										// check for valid file type
										bool pass = false;
										foreach (var type in fileTypes)
										{
											if (Path.GetExtension(fileName) == type)
											{
												pass = true;
												break;
											}
										}
										if (!pass) throw new Exception("Invalid file ext.");
									
										// load file
										MemoryStream stream = null;
										using (var file = new FileStream(path+fileName, FileMode.Open, FileAccess.Read))
										{
											if (maxWidth != 0 && maxHeight != 0)
											{
												ImageTools.IO.IImageDecoder decoder = null;
												switch (Path.GetExtension(fileName).ToLower())
												{
													case ".jpg": decoder = new ImageTools.IO.Jpeg.JpegDecoder(); break;
													case ".jpeg": decoder = new ImageTools.IO.Jpeg.JpegDecoder(); break;
													case ".png": decoder = new ImageTools.IO.Png.PngDecoder(); break;
													default:
														Debug.LogError("Unsuported file ext type: " + Path.GetExtension(fileName));
														streamLoadedCallback(null, false);
														return;
												}
												var image = new ImageTools.ExtendedImage();
												decoder.Decode(image, file);
												var newSize = MathUtilities.FitInViewIfLarger(image.PixelWidth, image.PixelHeight, maxWidth, maxHeight);
												var newImage = ImageTools.ExtendedImage.Resize(image, (int)newSize.x, (int)newSize.y, new ImageTools.Filtering.NearestNeighborResizer());
												var encoder = new ImageTools.IO.Jpeg.JpegEncoder();
												stream = new MemoryStream();
												encoder.Encode(newImage, stream);
												stream.Position = 0;

												/*unsafe
												{
													IntPtr ilib = IntPtr.Zero;
													if (img_lib_attach(&ilib) != 0)
													{
														Debug.LogError("Failed: img_lib_attach");
														streamLoadedCallback(null, false);
														return;
													}

													img_t image = new img_t();
													image.flags = 0x00000002;
													image.format = 32 | 0x00000100 | 0x00001000;
													img_decode_callouts_t callouts = new img_decode_callouts_t();
													if (img_load_file(ilib, "file://"+path+fileName, &callouts, &image) != 0)
													{
														img_t newImage = new img_t();
														if (img_resize_fs(&image, &newImage) != 0)
														{
															Debug.LogError("Failed: img_resize_fs");
															streamLoadedCallback(null, false);
															return;
														}

														Debug.Log("WIDTH: " + image.w);
														Debug.Log("HEIGHT: " + image.h);
														streamLoadedCallback(null, false);
														return;
														//byte* data = (byte*)newImage.data.ToPointer();
														//byte[] managedData = new byte[newImage.stride];

														//stream = new MemoryStream();
													}
													else
													{
														Debug.LogError("Failed to load image file: " + path + fileName);
														streamLoadedCallback(null, false);
														return;
													}

													img_lib_detach(ilib);
												}*/
											}
											else
											{
												var data = new byte[file.Length];
												file.Read(data, 0, data.Length);
												stream = new MemoryStream(data);
												stream.Position = 0;
											}
										}

										streamLoadedCallback(stream, true);
										return;
									}
									else
									{
										throw new Exception("Invalid dataPath.");
									}
								}
							}
							catch (Exception e)
							{
								Debug.LogError(e.Message);
							}
						
							streamLoadedCallback(null, false);
						}
						else
						{
							streamLoadedCallback(null, false);
						}
					
						break;
					}
				}
			}
		}
	}
}
#endif