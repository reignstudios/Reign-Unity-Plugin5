package com.reignstudios.reignnative;

import java.util.ArrayList;
import java.util.List;

//import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.widget.RelativeLayout;

public class ReignUnityActivity extends UnityPlayerActivity
{
	private static final String logTag = "Reign_Activity";
	public static ReignUnityActivity ReignContext;
	public static RelativeLayout ContentView;
	private List<ReignActivityCallbacks> callbacks;
	
	protected void onCreate(Bundle savedInstanceState)
	{
		// call UnityPlayerActivity.onCreate()
		super.onCreate(savedInstanceState);
		
		//UnityPlayer.currentActivity.
		
		ReignContext = this;
		callbacks = new ArrayList<ReignActivityCallbacks>();
		
		ContentView = new RelativeLayout(this);
		RelativeLayout.LayoutParams params = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MATCH_PARENT, RelativeLayout.LayoutParams.MATCH_PARENT);
		addContentView(ContentView, params);
		
		Log.d(logTag, "onCreate called!");
	}
	
	public static void AddCallbacks(ReignActivityCallbacks callbacks)
	{
		ReignContext.callbacks.add(callbacks);
	}
	
	public static void RemoveCallbacks(ReignActivityCallbacks callbacks)
	{
		ReignContext.callbacks.remove(callbacks);
	}
	
	@Override
	public void onBackPressed()
	{
		super.onBackPressed();
	}
	
    @Override
    public void onStart()
    {
    	Log.d(logTag, "onStart called!");
        super.onStart();
    }
    
    @Override
    protected void onPause()
    {
    	Log.d(logTag, "onPause called!");
    	for (ReignActivityCallbacks callback : callbacks)
		{
			callback.onPause();
		}
    	super.onPause();
    }

    @Override
    protected void onResume()
    {
    	Log.d(logTag, "onResume called!");
    	for (ReignActivityCallbacks callback : callbacks)
		{
			callback.onResume();
		}
        super.onResume();
    };
    
    @Override
    protected void onDestroy()
    {
    	Log.d(logTag, "onDestroy called!");
    	ReignContext = null;
    	ContentView = null;
    	callbacks = null;
    	super.onDestroy();
    };
	
	@Override
	protected void onActivityResult(int requestCode, int resultcode, Intent intent)
	{
		boolean canHandle = true;
		for (ReignActivityCallbacks callback : callbacks)
		{
			if (callback.onActivityResult(requestCode, resultcode, intent)) canHandle = false;
		}
		
		if (canHandle) super.onActivityResult(requestCode, resultcode, intent);
	}
}
