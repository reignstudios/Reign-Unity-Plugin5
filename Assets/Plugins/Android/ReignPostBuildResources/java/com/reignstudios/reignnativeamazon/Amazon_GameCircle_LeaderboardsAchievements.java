package com.reignstudios.reignnativeamazon;

import java.util.ArrayList;
import java.util.EnumSet;
import java.util.List;

import android.content.Intent;
import android.util.Log;

import com.amazon.ags.api.AGResponseCallback;
import com.amazon.ags.api.AGResponseHandle;
import com.amazon.ags.api.AmazonGamesCallback;
import com.amazon.ags.api.AmazonGamesClient;
import com.amazon.ags.api.AmazonGamesFeature;
import com.amazon.ags.api.AmazonGamesStatus;
import com.amazon.ags.api.achievements.Achievement;
import com.amazon.ags.api.achievements.AchievementsClient;
import com.amazon.ags.api.achievements.GetAchievementsResponse;
import com.amazon.ags.api.achievements.UpdateProgressResponse;
import com.amazon.ags.api.leaderboards.LeaderboardsClient;
import com.amazon.ags.api.leaderboards.SubmitScoreResponse;
import com.amazon.ags.api.player.RequestPlayerResponse;

import com.reignstudios.reignnative.ReignActivityCallbacks;
import com.reignstudios.reignnative.ReignAndroidPlugin;

public class Amazon_GameCircle_LeaderboardsAchievements implements AmazonGamesCallback, ReignActivityCallbacks
{
	private static final String logTag = "Reign_Amazon_GameCircle";
	private static Amazon_GameCircle_LeaderboardsAchievements singleton;
	private static AmazonGamesClient client;
	private static List<String> events;
	private static EnumSet<AmazonGamesFeature> gameFeatures;
	private static boolean isAthenticated, loggedOut;
	private static String requestAchievementsResult;
	
	public static void Init()
	{
		if (singleton == null) singleton = new Amazon_GameCircle_LeaderboardsAchievements();
		if (events == null) events = new ArrayList<String>();
	}
	
	public static boolean CheckIsAuthenticated()
	{
		return isAthenticated;
	}
	
	public static void Authenticate()
	{
		loggedOut = false;
		ReignAndroidPlugin.RootActivity.runOnUiThread(new Runnable()
		{
			public void run()
			{
				try
				{
					gameFeatures = EnumSet.of(AmazonGamesFeature.Achievements, AmazonGamesFeature.Leaderboards);
					AmazonGamesClient.initialize(ReignAndroidPlugin.RootActivity, singleton, gameFeatures);
				}
				catch (Exception e)
				{
					Log.i(logTag, "Authentication error: " + e.getMessage());
					events.add("Error:" + e.getMessage());
				}
			}
		});
	}
	
	public static void Logout()
	{
		loggedOut = true;
		isAthenticated = false;
		ReignAndroidPlugin.RootActivity.runOnUiThread(new Runnable()
		{
			public void run()
			{
				if (client != null)
				{
					AmazonGamesClient.release();
					client = null;
			    }
			}
		});
	}
	
	public static void ReportAchievement(final String id, final float percentComplete)
	{
		ReignAndroidPlugin.RootActivity.runOnUiThread(new Runnable()
		{
			public void run()
			{
				AchievementsClient acClient = client.getAchievementsClient();
				AGResponseHandle<UpdateProgressResponse> handle = acClient.updateProgress(id, percentComplete);
				 
				// Optional callback to receive notification of success/failure.
				handle.setCallback(new AGResponseCallback<UpdateProgressResponse>()
				{
				   @Override
				   public void onComplete(UpdateProgressResponse result)
				   {
				       if (result.isError())
				       {
				    	   Log.i(logTag, "Report achievement error: " + result.getError());
				    	   events.add("ReportAchievement:Failed");
				       }
				       else
				       {
				    	   events.add("ReportAchievement:Success");
				       }
				   }
				});
			}
		});
	}
	
	public static void ReportScore(final String id, final long scoreValue)
	{
		ReignAndroidPlugin.RootActivity.runOnUiThread(new Runnable()
		{
			public void run()
			{
				LeaderboardsClient lbClient = client.getLeaderboardsClient();
				AGResponseHandle<SubmitScoreResponse> handle = lbClient.submitScore(id, scoreValue);
				handle.setCallback(new AGResponseCallback<SubmitScoreResponse>()
				{
				    @Override
				    public void onComplete(SubmitScoreResponse result)
				    {
				        if (result.isError())
				        {
				        	Log.i(logTag, "Report score error: " + result.getError());
				        	events.add("ReportScore:Failed");
				        }
				        else
				        {
				        	events.add("ReportScore:Success");
				        }
				    }
				});
			}
		});
	}
	
	public static void RequestAchievements()
	{
		requestAchievementsResult = "";
		ReignAndroidPlugin.RootActivity.runOnUiThread(new Runnable()
		{
			public void run()
			{
				AchievementsClient acClient = client.getAchievementsClient();
				AGResponseHandle<GetAchievementsResponse> handle = acClient.getAchievements();
				handle.setCallback(new AGResponseCallback<GetAchievementsResponse>()
				{
					@Override
					public void onComplete(GetAchievementsResponse result)
					{
						if (result.isError())
				        {
				        	Log.i(logTag, "RequestAchievements Failed: " + result.getError());
				        	events.add("RequestAchievements:Failed");
				        }
				        else
				        {
				        	Log.i(logTag, "RequestAchievements Succeeded!");
							List<Achievement> achievements = result.getAchievementsList();
							boolean starting = true;
							if (achievements != null)
							for (int i = 0; i != achievements.size(); ++i)
							{
								Achievement a = achievements.get(i);
								if (!starting) requestAchievementsResult += ":";
								starting = false;
								requestAchievementsResult += a.getId();
								requestAchievementsResult += ":" + a.getProgress();
							}
							
							events.add("RequestAchievements:Success");
				        }
					}
				});
			}
		});
	}
	
	public static String GetRequestAchievementsResult()
	{
		return requestAchievementsResult;
	}
	
	public static void ShowNativeAchievementsPage()
	{
		ReignAndroidPlugin.RootActivity.runOnUiThread(new Runnable()
		{
			public void run()
			{
				client.getAchievementsClient().showAchievementsOverlay();
				events.add("ShowNativePage:Success");
			}
		});
	}
	
	public static void ShowNativeScoresPage(final String id)
	{
		ReignAndroidPlugin.RootActivity.runOnUiThread(new Runnable()
		{
			public void run()
			{
				client.getLeaderboardsClient().showLeaderboardsOverlay(id);
				events.add("ShowNativePage:Success");
			}
		});
	}

	@Override
	public void onServiceNotReady(AmazonGamesStatus arg0)
	{
		Log.i(logTag, "Service not Ready");
		isAthenticated = false;
		events.add("Error:ServiceNotReady");
	}

	@Override
	public void onServiceReady(AmazonGamesClient arg0)
	{
		client = arg0;
		client.getPlayerClient().getLocalPlayer((Object[])null).setCallback(new AGResponseCallback<RequestPlayerResponse>()
		{
			@Override
			public void onComplete(RequestPlayerResponse result)
			{
				if (result.isError())
				{
					Log.i(logTag, "GameCircleGetPlayerAlias ERROR: " + result.getError());
					events.add("Error:" + result.getError());
				} 
				else
				{
					String username = result.getPlayer().getAlias();
					events.add("Connected:" + username);
					isAthenticated = true;
				}
			}
		});
	}
	
	public static boolean HasEvents()
	{
		return events.size() != 0;
	}
	
	public static String GetNextEvent()
	{
		int index = events.size()-1;
		String next = events.get(index);
		events.remove(index);
		return next;
	}

	@Override
	public boolean onActivityResult(int arg0, int arg1, Intent arg2)
	{
		return false;
	}

	@Override
	public void onPause()
	{
		if (client != null)
		{
			isAthenticated = false;
			AmazonGamesClient.release();
			client = null;
	    }
	}
	
	@Override
	public void onResume()
	{
		if (client == null && !loggedOut)
		{
			AmazonGamesClient.initialize(ReignAndroidPlugin.RootActivity, singleton, gameFeatures);
		}
	}
}
