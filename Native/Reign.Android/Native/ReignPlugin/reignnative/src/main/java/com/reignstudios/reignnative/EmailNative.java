package com.reignstudios.reignnative;

import android.content.Intent;

public class EmailNative
{
	public static void Send(final String to, final String subject, final String body)
	{
		ReignUnityActivity.ReignContext.runOnUiThread(new Runnable()
		{
			public void run()
			{
				Intent email = new Intent(Intent.ACTION_SEND);
				email.putExtra(Intent.EXTRA_EMAIL, new String[]{to});
				email.putExtra(Intent.EXTRA_SUBJECT, subject);
				email.putExtra(Intent.EXTRA_TEXT, body);
				email.setType("message/rfc822");
				ReignUnityActivity.ReignContext.startActivity(email);
			}
		});
	}
}
