package com.reignstudios.reignnative;

import java.util.ArrayList;
import java.util.List;

import android.app.Activity;
import android.content.Intent;
import android.util.Log;
import android.widget.RelativeLayout;

public class ReignAndroidPlugin
{
	private static final String logTag = "Reign_Activity";
	public static Activity RootActivity;
	public static RelativeLayout ContentView;
	private static List<ReignActivityCallbacks> callbacks;
	
	public static void Init(Activity activity)
	{
		RootActivity = activity;
		callbacks = new ArrayList();
		
		ContentView = new RelativeLayout(activity);
		RelativeLayout.LayoutParams params = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MATCH_PARENT, RelativeLayout.LayoutParams.MATCH_PARENT);
		activity.addContentView(ContentView, params);
		
		Log.d(logTag, "onCreate called!");
	}
	
	public static void AddCallbacks(ReignActivityCallbacks callbacksToAdd)
	{
		callbacks.add(callbacksToAdd);
	}
	
	public static void RemoveCallbacks(ReignActivityCallbacks callbacksToRemove)
	{
		callbacks.remove(callbacksToRemove);
	}

	public static void onPause()
    {
    	Log.d(logTag, "onPause called!");
    	for (ReignActivityCallbacks callback : callbacks)
		{
			callback.onPause();
		}
    }

	public static void onResume()
    {
    	Log.d(logTag, "onResume called!");
    	for (ReignActivityCallbacks callback : callbacks)
		{
			callback.onResume();
		}
    };

    public static void onDestroy()
    {
    	Log.d(logTag, "onDestroy called!");
		RootActivity = null;
    	ContentView = null;
    	callbacks = null;
    };

	public static boolean onActivityResult(int requestCode, int resultcode, Intent intent)
	{
		boolean canHandle = true;
		for (ReignActivityCallbacks callback : callbacks)
		{
			if (callback.onActivityResult(requestCode, resultcode, intent)) canHandle = false;
		}

		return canHandle;
	}
}
