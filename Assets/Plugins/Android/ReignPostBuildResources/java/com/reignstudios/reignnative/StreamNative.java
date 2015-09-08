package com.reignstudios.reignnative;

import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.OutputStream;
import java.io.RandomAccessFile;
import java.text.SimpleDateFormat;
import java.util.Date;

import android.app.Activity;
import android.content.ContentResolver;
import android.content.ContentUris;
import android.content.ContentValues;
import android.content.Context;
import android.content.Intent;
import android.database.Cursor;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Matrix;
import android.net.Uri;
import android.os.Environment;
import android.provider.MediaStore;
import android.provider.MediaStore.Images;
import android.provider.MediaStore.Images.ImageColumns;
import android.util.Log;

public class StreamNative implements ReignActivityCallbacks
{
	private static final String logTag = "Reign_Streams";
	private static StreamNative singleton;
	private static boolean saveImageDone, saveImageSucceeded;
	private static boolean loadImageDone, loadImageSucceeded;
	public static int LoadImageIntentID = 123, LoadCameraPickerIntent = 124;
	private static byte[] loadImageData;
	private static Uri cameraImagePath;
	
	public static void Init()
	{
		singleton = new StreamNative();
		ReignAndroidPlugin.AddCallbacks(singleton);
	}
	
	public static void SaveImage(final byte[] data, final String title, final String desc)
	{
		saveImageSucceeded = false;
		saveImageDone = false;

		ReignAndroidPlugin.RootActivity.runOnUiThread(new Runnable()
		{
			public void run()
			{
				try
				{
					Bitmap image = BitmapFactory.decodeByteArray(data, 0, data.length);
					//MediaStore.Images.Media.insertImage(ReignAndroidPlugin.RootActivity.getContentResolver(), image, title, desc);
					insertImage(ReignAndroidPlugin.RootActivity.getContentResolver(), image, title, desc);// Google implementation saves at 50% quality
					saveImageSucceeded = true;
					saveImageDone = true;
				}
				catch (Exception e)
				{
					Log.d(logTag, "SaveImage Error: " + e.getMessage());
					saveImageSucceeded = false;
					saveImageDone = true;
				}
			}
		});
	}
	
	public static final String insertImage(ContentResolver cr, Bitmap source, String title, String description)
	{
		ContentValues values = new ContentValues();
		values.put(Images.Media.TITLE, title);
		values.put(Images.Media.DESCRIPTION, description);
		values.put(Images.Media.MIME_TYPE, "image/jpeg");
		
		long date = System.currentTimeMillis();
		values.put(Images.Media.DATE_TAKEN, date);
		values.put(Images.Media.DATE_ADDED, date / 1000);
		
		Uri url = null;
		String stringUrl = null;    /* value to be returned */
		
		try
		{
			url = cr.insert(MediaStore.Images.Media.EXTERNAL_CONTENT_URI, values);
			
			if (source != null)
			{
				OutputStream imageOut = cr.openOutputStream(url);
				try
				{
					source.compress(Bitmap.CompressFormat.JPEG, 100, imageOut);
				}
				finally
				{
					imageOut.close();
				}
			
				long id = ContentUris.parseId(url);
				// Wait until MINI_KIND thumbnail is generated.
				Bitmap miniThumb = Images.Thumbnails.getThumbnail(cr, id,
				Images.Thumbnails.MINI_KIND, null);
				// This is for backward compatibility.
				Bitmap microThumb = StoreThumbnail(cr, miniThumb, id, 50F, 50F,
				Images.Thumbnails.MICRO_KIND);
			}
			else
			{
				Log.e(logTag, "Failed to create thumbnail, removing original");
				cr.delete(url, null, null);
				url = null;
			}
		}
		catch (Exception e)
		{
			Log.e(logTag, "Failed to insert image", e);
			if (url != null)
			{
				cr.delete(url, null, null);
				url = null;
			}
		}
		
		if (url != null) stringUrl = url.toString();
		return stringUrl;
	}
	
	private static final Bitmap StoreThumbnail(ContentResolver cr, Bitmap source, long id, float width, float height, int kind)
	{
        // create the matrix to scale it
        Matrix matrix = new Matrix();

        float scaleX = width / source.getWidth();
        float scaleY = height / source.getHeight();

        matrix.setScale(scaleX, scaleY);

        Bitmap thumb = Bitmap.createBitmap(source, 0, 0,
                                           source.getWidth(),
                                           source.getHeight(), matrix,
                                           true);

        ContentValues values = new ContentValues(4);
        values.put(Images.Thumbnails.KIND,     kind);
        values.put(Images.Thumbnails.IMAGE_ID, (int)id);
        values.put(Images.Thumbnails.HEIGHT,   thumb.getHeight());
        values.put(Images.Thumbnails.WIDTH,    thumb.getWidth());

        Uri url = cr.insert(Images.Thumbnails.EXTERNAL_CONTENT_URI, values);

        try
        {
            OutputStream thumbOut = cr.openOutputStream(url);

            thumb.compress(Bitmap.CompressFormat.JPEG, 100, thumbOut);
            thumbOut.close();
            return thumb;
        }
        catch (FileNotFoundException ex)
        {
            return null;
        }
        catch (IOException ex) 
       {
            return null;
        }
    }
	
	private static float[] fitInViewIfLarger(float objectWidth, float objectHeight, float viewWidth, float viewHeight)
	{
		if (objectWidth <= viewWidth && objectHeight <= viewHeight) return new float[]{objectWidth, objectHeight};
		
		float objectSlope = objectHeight / objectWidth;
		float viewSlope = viewHeight / viewWidth;
	
		if (objectSlope >= viewSlope) return new float[]{(objectWidth/objectHeight) * viewHeight, viewHeight};
		else return new float[]{viewWidth, (objectHeight/objectWidth) * viewWidth};
	}
	
	private static int LoadImage_maxWidth, LoadImage_maxHeight;
	public static void LoadImage(final int maxWidth, final int maxHeight)
	{
		loadImageSucceeded = false;
		loadImageDone = false;
		
		LoadImage_maxWidth = maxWidth;
		LoadImage_maxHeight = maxHeight;
		ReignAndroidPlugin.RootActivity.runOnUiThread(new Runnable()
		{
			public void run()
			{
				try
				{
					Intent photoPickerIntent = new Intent(Intent.ACTION_PICK);
					photoPickerIntent.setType("image/*");
					ReignAndroidPlugin.RootActivity.startActivityForResult(photoPickerIntent, LoadImageIntentID);
				}
				catch (Exception e)
				{
					Log.d(logTag, "LoadImage Error: " + e.getMessage());
					loadImageSucceeded = false;
					loadImageDone = true;
				}
			}
		});
	}
	
	public static void LoadCameraPicker(final int maxWidth, final int maxHeight)
	{
		loadImageSucceeded = false;
		loadImageDone = false;
		
		LoadImage_maxWidth = maxWidth;
		LoadImage_maxHeight = maxHeight;
		ReignAndroidPlugin.RootActivity.runOnUiThread(new Runnable()
		{
			public void run()
			{
				try
				{
					// Older devices (Android 2) may need this method! (Don't know how to make that determination though)
					/*String timeStamp = new SimpleDateFormat("yyyyMMdd_HHmmss").format(new Date());
					File file = new File(Environment.getExternalStorageDirectory() + "/DCIM", "IMG_" + timeStamp + ".jpg");
					file.mkdirs();
					cameraImagePath = Uri.fromFile(file);
					Log.d(logTag, "LoadCameraPicker cameraImagePath: " + cameraImagePath);*/
					
					// get camera image file path
			        String timeStamp = new SimpleDateFormat("yyyyMMdd_HHmmss").format(new Date());
			        ContentValues values = new ContentValues();
			        String name = "IMG_" + timeStamp + ".jpg";
			        values.put(MediaStore.Images.Media.TITLE, name);
			        cameraImagePath = ReignAndroidPlugin.RootActivity.getContentResolver().insert(MediaStore.Images.Media.EXTERNAL_CONTENT_URI, values);
			        String path = getRealPathFromURI(cameraImagePath);
					Log.d(logTag, "LoadCameraPicker cameraImagePath: " + path);
					
					// invoke camera
					Intent photoPickerIntent = new Intent(MediaStore.ACTION_IMAGE_CAPTURE);
					photoPickerIntent.putExtra(MediaStore.EXTRA_OUTPUT, cameraImagePath);
					ReignAndroidPlugin.RootActivity.startActivityForResult(photoPickerIntent, LoadCameraPickerIntent);
				}
				catch (Exception e)
				{
					Log.d(logTag, "LoadCameraPicker Error: " + e.getMessage());
					loadImageSucceeded = false;
					loadImageDone = true;
				}
			}
		});
	}
	
	private static String getRealPathFromURI(Uri contentUri)
	{
		Cursor cursor = null;
		try
		{ 
			String[] proj = { MediaStore.Images.Media.DATA };
			cursor = ReignAndroidPlugin.RootActivity.getContentResolver().query(contentUri, proj, null, null, null);
			int column_index = cursor.getColumnIndexOrThrow(MediaStore.Images.Media.DATA);
			cursor.moveToFirst();
			return cursor.getString(column_index);
		}
		finally
		{
			if (cursor != null) cursor.close();
		}
	}
	
	@Override
	public boolean onActivityResult(int requestCode, int resultcode, Intent intent)
	{
		if (requestCode == LoadImageIntentID)
		{
			if (resultcode != Activity.RESULT_OK)
			{
				loadImageSucceeded = false;
				loadImageDone = true;
				return true;
			}
			
			try
			{
				if (intent != null)
				{
					Uri data = intent.getData();
					if (data == null)
					{
						Log.d(logTag, "LoadImage onActivityResult Error: Intent was null");
						loadImageSucceeded = false;
						loadImageDone = true;
						return true;
					}
					
					Cursor cursor = ReignAndroidPlugin.RootActivity.getContentResolver().query(data, null, null, null, null);
					cursor.moveToFirst();
					int idx = cursor.getColumnIndex(ImageColumns.DATA);
					String fileName = cursor.getString(idx);
					
					loadImageData = readFile(fileName);
					
					// resize image if needed
					if (LoadImage_maxWidth != 0 && LoadImage_maxHeight != 0)
					{
						Bitmap bmp = BitmapFactory.decodeByteArray(loadImageData, 0, loadImageData.length);
						float[] newSize = fitInViewIfLarger(bmp.getWidth(), bmp.getHeight(), LoadImage_maxWidth, LoadImage_maxHeight);
						
						bmp = Bitmap.createScaledBitmap(bmp, (int)newSize[0], (int)newSize[1], true);
						ByteArrayOutputStream stream = new ByteArrayOutputStream();
						bmp.compress(Bitmap.CompressFormat.JPEG, 95, stream);
						loadImageData = stream.toByteArray();
						stream.close();
					}
					
					loadImageSucceeded = true;
					loadImageDone = true;
					
					// REMEMBER for getting pixel data...
					//Bitmap bitmapPreview = BitmapFactory.decodeFile(fileName);
					//bitmapPreview.copyPixelsToBuffer(dst)// <<< REMEMBER
				}
				else
				{
					Log.d(logTag, "LoadImage onActivityResult Error: Intent was null");
					loadImageSucceeded = false;
					loadImageDone = true;
				}
			}
			catch (Exception e)
			{
				Log.d(logTag, "LoadImage onActivityResult Error: " + e.getMessage());
				loadImageSucceeded = false;
				loadImageDone = true;
			}
		}
		else if (requestCode == LoadCameraPickerIntent)
		{
			if (resultcode != Activity.RESULT_OK)
			{
				loadImageSucceeded = false;
				loadImageDone = true;
				return true;
			}
			
			try
			{
				loadImageData = readFile(getRealPathFromURI(cameraImagePath));
				
				// resize image if needed
				if (LoadImage_maxWidth != 0 && LoadImage_maxHeight != 0)
				{
					Bitmap bmp = BitmapFactory.decodeByteArray(loadImageData, 0, loadImageData.length);
					float[] newSize = fitInViewIfLarger(bmp.getWidth(), bmp.getHeight(), LoadImage_maxWidth, LoadImage_maxHeight);
					
					bmp = Bitmap.createScaledBitmap(bmp, (int)newSize[0], (int)newSize[1], true);
					ByteArrayOutputStream stream = new ByteArrayOutputStream();
					bmp.compress(Bitmap.CompressFormat.JPEG, 95, stream);
					loadImageData = stream.toByteArray();
					stream.close();
				}
				
				loadImageSucceeded = true;
				loadImageDone = true;
			}
			catch (Exception e)
			{
				Log.d(logTag, "LoadCameraPicker onActivityResult Error: " + e.getMessage());
				loadImageSucceeded = false;
				loadImageDone = true;
			}
		}
		
		return true;
	}
	
	@Override
	public void onPause()
	{
		// do nothing...
	}
	
	@Override
	public void onResume()
	{
		// do nothing...
	}
	
	private static byte[] readFile(String fileName) throws IOException
	{
        // Open file
		RandomAccessFile f = null;
        try
        {
        	f = new RandomAccessFile(fileName, "r");
        	
            // Get and check length
            long longlength = f.length();
            int length = (int)longlength;
            if (length != longlength) throw new IOException("File size >= 2 GB");
            
            // Read file and return data
            byte[] data = new byte[length];
            f.readFully(data);
            return data;
        }
        catch (Exception e)
        {
        	Log.d(logTag, "readFile Error: " + e.getMessage());
        	return null;
        }
        finally
        {
            if (f != null) f.close();
        }
    }
	
	public static boolean CheckSaveImageDone()
	{
		Boolean d = saveImageDone;
		saveImageDone = false;
		return d;
	}
	
	public static boolean CheckSaveImageSucceeded()
	{
		return saveImageSucceeded;
	}
	
	public static boolean CheckLoadImageDone()
	{
		Boolean d = loadImageDone;
		loadImageDone = false;
		return d;
	}
	
	public static boolean CheckLoadImageSucceeded()
	{
		return loadImageSucceeded;
	}
	
	public static byte[] GetLoadedImageData()
	{
		byte[] data = loadImageData;
		loadImageData = null;
		return data;
	}
}
