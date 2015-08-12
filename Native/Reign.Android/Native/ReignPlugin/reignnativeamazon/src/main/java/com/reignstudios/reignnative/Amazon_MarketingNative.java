package com.reignstudios.reignnative;

import android.content.Intent;
import android.net.Uri;

public class Amazon_MarketingNative
{
	public static void OpenStore(final String url)
	{
		ReignUnityActivity.ReignContext.runOnUiThread(new Runnable()
		{
			public void run()
			{
				Uri marketUri = Uri.parse("amzn://apps/android?p=" + url);
				Intent i = new Intent(Intent.ACTION_VIEW, marketUri);
				ReignUnityActivity.ReignContext.startActivity(i);
			}
		});
	}
}
