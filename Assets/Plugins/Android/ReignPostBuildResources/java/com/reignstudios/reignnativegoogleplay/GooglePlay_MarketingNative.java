package com.reignstudios.reignnativegoogleplay;

import android.content.Intent;
import android.net.Uri;

import com.reignstudios.reignnative.ReignAndroidPlugin;

public class GooglePlay_MarketingNative
{
	public static void OpenStore(final String url)
	{
		ReignAndroidPlugin.RootActivity.runOnUiThread(new Runnable()
		{
			public void run()
			{
				Uri marketUri = Uri.parse("market://details?id=" + url);
				Intent i = new Intent(Intent.ACTION_VIEW, marketUri);
				ReignAndroidPlugin.RootActivity.startActivity(i);
			}
		});
	}
}
