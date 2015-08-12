package com.reignstudios.reignnative;

import java.io.File;
import java.io.FileOutputStream;

import android.content.Intent;
import android.net.Uri;
import android.os.Environment;
import android.util.Log;

public class SocialNative
{
	private static final String logTag = "Reign_Social";
	
	public static void ShareImage(final byte[] data, final String dataFilename, final String text, final String title, final boolean isPNG)
	{
		ReignUnityActivity.ReignContext.runOnUiThread(new Runnable()
		{
			public void run()
			{
				if (data != null)
				{
					Log.d(logTag, "Saving file for ShareImage dlg");
					File sdCardDirectory = Environment.getExternalStorageDirectory();
					File imageFile = new File(sdCardDirectory, isPNG ? (dataFilename+".png") : (dataFilename+".jpg"));
					FileOutputStream outputStream;
					try
					{
						outputStream = new FileOutputStream(imageFile);
						outputStream.write(data);
						outputStream.close();
					}
					catch (Exception e)
					{
						Log.d(logTag, "ShareImage error!");
						e.printStackTrace();
					}
					
					Log.d(logTag, "Invoking ShareImage dlg: IsPNG = " + isPNG);
					Log.d(logTag, "Data length = " + data.length);
					
					if (text != null && text.length() != 0)
					{
						Intent shareIntent = new Intent(Intent.ACTION_SEND);
						//shareIntent.setType("*/*");
						if (isPNG) shareIntent.setType("image/png");
						else shareIntent.setType("image/jpg");
						shareIntent.putExtra(Intent.EXTRA_STREAM, Uri.fromFile(imageFile));
						shareIntent.putExtra(Intent.EXTRA_TEXT, text);
						shareIntent.putExtra(Intent.EXTRA_SUBJECT, title);
						Intent chooser = Intent.createChooser(shareIntent, title);
						ReignUnityActivity.ReignContext.startActivity(chooser);
					}
					else
					{
						Intent shareIntent = new Intent(Intent.ACTION_SEND);
						shareIntent.putExtra(Intent.EXTRA_STREAM, Uri.fromFile(imageFile));
						shareIntent.putExtra(Intent.EXTRA_SUBJECT, title);
						if (isPNG) shareIntent.setType("image/png");
						else shareIntent.setType("image/jpg");
						Intent chooser = Intent.createChooser(shareIntent, title);
						ReignUnityActivity.ReignContext.startActivity(chooser);
					}
				}
				else if (text != null && text.length() != 0)
				{
					Intent shareIntent = new Intent(Intent.ACTION_SEND);
					shareIntent.putExtra(Intent.EXTRA_TEXT, text);
					shareIntent.putExtra(Intent.EXTRA_SUBJECT, title);
					shareIntent.setType("text/plain");
					Intent chooser = Intent.createChooser(shareIntent, title);
					ReignUnityActivity.ReignContext.startActivity(chooser);
				}
			}
		});
	}
}
