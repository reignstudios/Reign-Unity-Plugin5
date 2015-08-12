package com.reignstudios.reignnative;

import java.util.ArrayList;
import java.util.List;

import android.app.Activity;
import android.content.Intent;
import android.content.IntentSender.SendIntentException;
import android.os.Bundle;
import android.util.Log;

import com.google.android.gms.appstate.AppStateManager;
import com.google.android.gms.common.ConnectionResult;
import com.google.android.gms.common.api.GoogleApiClient;
import com.google.android.gms.common.api.PendingResult;
import com.google.android.gms.common.api.ResultCallback;
import com.google.android.gms.games.Games;
import com.google.android.gms.games.GamesActivityResultCodes;
import com.google.android.gms.games.GamesStatusCodes;
import com.google.android.gms.games.achievement.Achievement;
import com.google.android.gms.games.achievement.AchievementBuffer;
import com.google.android.gms.games.achievement.Achievements.LoadAchievementsResult;
import com.google.android.gms.games.achievement.Achievements.UpdateAchievementResult;
import com.google.android.gms.games.leaderboard.Leaderboards.SubmitScoreResult;
import com.google.android.gms.plus.Plus;

public class GooglePlay_LeaderboardsAchievements implements GoogleApiClient.ConnectionCallbacks, GoogleApiClient.OnConnectionFailedListener, ReignActivityCallbacks
{
	private static final String logTag = "Reign_GooglePlay_Scores";
	private static GooglePlay_LeaderboardsAchievements singleton;
	private static GoogleApiClient client;
	private static List<String> events;
	private static int initStatus;
	private static boolean isAuthenticated, disableUsernameRetrieval;
	private static String requestAchievementsResult;
	
	private static int REQUEST_LEADERBOARD_ID = 100001, REQUEST_ACHIEVEMENTS_ID = 100002, RC_RESOLVE_ID = 100003;
	
	public static void Init(boolean disableUsernameRetrieval)
	{
		if (singleton == null) singleton = new GooglePlay_LeaderboardsAchievements();
		if (events == null) events = new ArrayList<String>();
		ReignUnityActivity.AddCallbacks(singleton);
		
		GooglePlay_LeaderboardsAchievements.disableUsernameRetrieval = disableUsernameRetrieval;
		ReignUnityActivity.ReignContext.runOnUiThread(new Runnable()
		{
			public void run()
			{
				try
				{
					GoogleApiClient.Builder builder = new GoogleApiClient.Builder(ReignUnityActivity.ReignContext, singleton, singleton);
	
					builder.addApi(Games.API)
			           .addApi(Plus.API)
			           .addApi(AppStateManager.API)
			           .addScope(Games.SCOPE_GAMES)
			           .addScope(Plus.SCOPE_PLUS_LOGIN)
			           .addScope(AppStateManager.SCOPE_APP_STATE)
			           .setViewForPopups(ReignUnityActivity.ContentView);
					
					client = builder.build();
					
					initStatus = 1;
				}
				catch (Exception e)
				{
					events.add("Error:" + e.getMessage());
				}
			}
		});
	}
	
	public static boolean CheckIsAuthenticated()
	{
		return isAuthenticated;
	}
	
	public static int CheckInitStatus()
	{
		int status = initStatus;
		initStatus = 0;
		return status;
	}
	
	public static void Authenticate()
	{
		ReignUnityActivity.ReignContext.runOnUiThread(new Runnable()
		{
			public void run()
			{
				try
				{
					isAuthenticated = client.isConnected();
					if (!isAuthenticated) client.connect();
				}
				catch (Exception e)
				{
					isAuthenticated = false;
					Log.i(logTag, "Error when trying to connect: " + e.toString());
					events.add("Error:AuthenticateFailed");
				}
			}
		});
	}
	
	public static void Logout()
	{
		isAuthenticated = false;
		ReignUnityActivity.ReignContext.runOnUiThread(new Runnable()
		{
			public void run()
			{
				try
				{
					if (client.isConnected())
					{
						Games.signOut(client);
						client.disconnect();
					}
				}
				catch (Exception e)
				{
					isAuthenticated = false;
					Log.i(logTag, "Error when trying to disconnect: " + e.toString());
					client.disconnect();
				}
			}
		});
	}
	
	private static boolean checkAuthenticationState()
	{
		try
		{
			if (!isAuthenticated || !client.isConnected())
			{
				isAuthenticated = false;
				Log.i(logTag, "Failed because user not logged in!");
				return false;
			}
		}
		catch (Exception e)
		{
			isAuthenticated = false;
			Log.i(logTag, "Authentication error: " + e.toString());
			client.disconnect();
			return false;
		}
		
		return true;
	}
	
	public static void ReportAchievement(final String id, final float percentComplete, final boolean isIncrementalType)
	{
		ReignUnityActivity.ReignContext.runOnUiThread(new Runnable()
		{
			public void run()
			{
				if (!checkAuthenticationState()) return;
				
				PendingResult<UpdateAchievementResult> result = null;
				if (isIncrementalType) result = Games.Achievements.setStepsImmediate(client, id, (int)percentComplete);
				else result = Games.Achievements.unlockImmediate(client, id);
				ResultCallback<UpdateAchievementResult> resultCallback = new ResultCallback<UpdateAchievementResult>()
				{
    		        @Override
    		        public void onResult(UpdateAchievementResult result)
    		        {
	    				if (result.getStatus().getStatusCode() == GamesStatusCodes.STATUS_OK || result.getStatus().getStatusCode() == GamesStatusCodes.STATUS_ACHIEVEMENT_UNLOCKED) events.add("ReportAchievement:Success");
	    				else events.add("ReportAchievement:Failed");
    		        }
    		    };
    		    
    		    result.setResultCallback(resultCallback);
			}
		});
	}
	
	public static void ReportScore(final String id, final long scoreValue)
	{
		ReignUnityActivity.ReignContext.runOnUiThread(new Runnable()
		{
			public void run()
			{
				if (!checkAuthenticationState()) return;
				
				PendingResult<SubmitScoreResult> result = Games.Leaderboards.submitScoreImmediate(client, id, scoreValue);//.setResultCallback(singleton);
				ResultCallback<SubmitScoreResult> resultCallback = new ResultCallback<SubmitScoreResult>()
				{
    		        @Override
    		        public void onResult(SubmitScoreResult result)
    		        {
	    				if (result.getStatus().getStatusCode() == GamesStatusCodes.STATUS_OK) events.add("ReportScore:Success");
	    				else events.add("ReportScore:Failed");
    		        }
    		    };
    		    
    		    result.setResultCallback(resultCallback);
			}
		});
	}
	
	public static void RequestAchievements()
	{
		requestAchievementsResult = "";
		ReignUnityActivity.ReignContext.runOnUiThread(new Runnable()
		{
			public void run()
			{
				if (!checkAuthenticationState()) return;
				
				PendingResult<LoadAchievementsResult> result = Games.Achievements.load(client, true);
				ResultCallback<LoadAchievementsResult> resultCallback = new ResultCallback<LoadAchievementsResult>()
				{
					@Override
					public void onResult(LoadAchievementsResult result)
					{
						if (result.getStatus().getStatusCode() == GamesStatusCodes.STATUS_OK)
						{
							Log.i(logTag, "RequestAchievements Succeeded!");
							AchievementBuffer achievements = result.getAchievements();
							boolean starting = true;
							if (achievements != null)
							for (int i = 0; i != achievements.getCount(); ++i)
							{
								Achievement a = achievements.get(i);
								if (!starting) requestAchievementsResult += ":";
								starting = false;
								requestAchievementsResult += a.getAchievementId();
								if (a.getType() == Achievement.TYPE_INCREMENTAL) requestAchievementsResult += ":" + a.getCurrentSteps();
								else requestAchievementsResult += ":" + (a.getState() == Achievement.STATE_UNLOCKED ? "Unlocked" : "NotUnlocked");
							}
							
							events.add("RequestAchievements:Success");
						}
						else
						{
							Log.i(logTag, "RequestAchievements Failed: " + result.getStatus());
				        	events.add("RequestAchievements:Failed");
						}
					}
				};
				
				result.setResultCallback(resultCallback);
			}
		});
	}
	
	public static String GetRequestAchievementsResult()
	{
		return requestAchievementsResult;
	}
	
	public static void ShowNativeAchievementsPage()
	{
		ReignUnityActivity.ReignContext.runOnUiThread(new Runnable()
		{
			public void run()
			{
				if (!checkAuthenticationState()) return;
				ReignUnityActivity.ReignContext.startActivityForResult(Games.Achievements.getAchievementsIntent(client), REQUEST_ACHIEVEMENTS_ID);
			}
		});
	}
	
	public static void ShowNativeScoresPage(final String id)
	{
		ReignUnityActivity.ReignContext.runOnUiThread(new Runnable()
		{
			public void run()
			{
				if (!checkAuthenticationState()) return;
				ReignUnityActivity.ReignContext.startActivityForResult(Games.Leaderboards.getLeaderboardIntent(client, id), REQUEST_LEADERBOARD_ID);
			}
		});
	}

	@Override
	public void onConnectionFailed(ConnectionResult arg0)
	{
		isAuthenticated = false;
		try
		{
			arg0.startResolutionForResult(ReignUnityActivity.ReignContext, RC_RESOLVE_ID);
		}
		catch (SendIntentException e)
		{
			e.printStackTrace();
			events.add("Error:ConnectionFailed with error code " + arg0.getErrorCode());
		}
	}

	@Override
	public void onConnected(Bundle arg0)
	{
		String username = "Disabled";
		if (!disableUsernameRetrieval) username = Plus.AccountApi.getAccountName(client);
		events.add("Connected:" + username);
		isAuthenticated = true;
	}

	@Override
	public void onConnectionSuspended(int cause)
	{
		isAuthenticated = false;
		events.add("Error:ConnectionSuspended");
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
	public boolean onActivityResult(int requestCode, int resultcode, Intent intent)
	{
		if (requestCode == REQUEST_ACHIEVEMENTS_ID || requestCode == REQUEST_LEADERBOARD_ID)
		{
			if (resultcode == GamesActivityResultCodes.RESULT_RECONNECT_REQUIRED)
			{
				Log.d(logTag, "Diconnected");
				isAuthenticated = false;
				client.disconnect();
				events.add("ShowNativePage:Success");
			}
			else if (resultcode == Activity.RESULT_OK || resultcode == Activity.RESULT_CANCELED)
			{
				events.add("ShowNativePage:Success");
			}
			else
			{
				events.add("ShowNativePage:Failed with code " + resultcode);
			}
			
			return true;
		}
		else if (requestCode == RC_RESOLVE_ID)
		{
			if (resultcode == Activity.RESULT_OK)
			{
				Log.d(logTag, "Trying to connect after RESULT_OK");
				isAuthenticated = false;
				client.connect();
			}
			else if (resultcode == GamesActivityResultCodes.RESULT_RECONNECT_REQUIRED)
			{
				Log.d(logTag, "Trying to connect after RESULT_RECONNECT_REQUIRED");
				isAuthenticated = false;
				client.connect();
			}
			else if (resultcode == Activity.RESULT_CANCELED)
			{
				Log.d(logTag, "Canceled connection");
				isAuthenticated = false;
				client.disconnect();
				events.add("Error:ConnectionCanceled");
			}
			else
			{
				Log.d(logTag, "Failed to connect");
				isAuthenticated = false;
				events.add("Error:FailedToConnect");
			}
			
			return true;
		}
		
		return false;
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
}
