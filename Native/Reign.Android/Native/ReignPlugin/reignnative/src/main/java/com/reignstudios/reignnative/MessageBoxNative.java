package com.reignstudios.reignnative;

import android.app.AlertDialog;
import android.content.DialogInterface;

public class MessageBoxNative
{
	private static boolean okClicked, cancelClicked;
	
	public static void Show(final String title, final String message, final String okButtonText, final String cancelButtonText, final int type)
	{
		ReignUnityActivity.ReignContext.runOnUiThread(new Runnable()
		{
			public void run()
			{
				AlertDialog.Builder dlgAlert = new AlertDialog.Builder(ReignUnityActivity.ReignContext);
				dlgAlert.setTitle(title);
				dlgAlert.setMessage(message);
				dlgAlert.setPositiveButton(okButtonText, new DialogInterface.OnClickListener()
				{
					public void onClick(DialogInterface dialog, int which)
					{
						okClicked = true;
					}
				});
				
				if (type == 1)
				{
					dlgAlert.setNegativeButton(cancelButtonText, new DialogInterface.OnClickListener()
					{
						public void onClick(DialogInterface dialog, int which)
						{
							cancelClicked = true;
						}
					});
				}
				
				dlgAlert.setCancelable(false);
				
				dlgAlert.show();
			}
		});
	}
	
	public static boolean GetOkStatus()
	{
		boolean value = okClicked;
		okClicked = false;
		return value;
	}
	
	public static boolean GetCancelStatus()
	{
		boolean value = cancelClicked;
		cancelClicked = false;
		return value;
	}
}
