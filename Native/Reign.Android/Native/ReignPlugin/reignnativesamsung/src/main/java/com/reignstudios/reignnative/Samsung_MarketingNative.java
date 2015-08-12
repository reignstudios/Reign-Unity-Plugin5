package com.reignstudios.reignnative;

import android.content.Intent;
import android.net.Uri;

public class Samsung_MarketingNative
{
	public static void OpenStore(final String url)
	{
		ReignUnityActivity.ReignContext.runOnUiThread(new Runnable()
		{
			public void run()
			{
				Uri marketUri = Uri.parse("samsungapps://ProductDetail/" + url);
				Intent i = new Intent(Intent.ACTION_VIEW, marketUri);
				ReignUnityActivity.ReignContext.startActivity(i);
			}
		});
	}
}
