For in depth documentation please go to: http://www.reign-studios.net/docs/
Post build steps will be explained there for each supported platform.

----------------------------
Getting Started
----------------------------
Open one of the scenes from: "Assets/Plugins/DemoScenes/...".
And look at ".cs" files for reference.
Make sure you have the "Assets/Plugins/Reign/Services/ReignServices.prefab" in the Scene for the plugins to work.

----------------------------
Pre/Post build steps
----------------------------
Look at "http://www.reign-studios.net/docs/" for potential pre/post build steps required for particular platforms.

----------------------------
Updating
----------------------------
If you have multiple Reign Unity Plugins its a good idea to check and make sure there versions are in sync via "Edit->Reign->Validate plugin versions are in Sync".
If they are not or you are having issues:
1) Download a up to date package via: "Edit->Reign->Download UnityPackage Updates".
2) Delete all Reign Plugin files.
3) Re-Install the Reign Unity Plugin package.

----------------------------
Disable unwanted features
----------------------------
If you don't want or need a particular feature you can set these compiler directives (Scripting Define Symbols) in Unity's PlayerSettings.
NOTE: DISABLE_REIGN will disable Reign for any platform and get rid of compiler issues for unsupported ones.
Android:
	ANDROID_DISABLE_GOOGLEPLAY - Disabled all GooglePlay features like AdMob, Leaderboard/Achievements, IAP, ect.
	ANDROID_DISABLE_AMAZON - Disabled all Amazon features like Ads, Leaderboard/Achievements, IAP, ect.
	ANDROID_DISABLE_SAMSUNG - Disabled all Samsung features like IAP, ect.
	ANDROID_DISABLE_AMAZON_ADS - Disables Amazon Ads.
	ANDROID_DISABLE_AMAZON_SCORES - Disables Amazon Leaderboards and Acheivements (Game Circle).
	ANDROID_DISABLE_AMAZON_IAP - Disables Amazon IAP.
	ANDROID_DISABLE_SAMSUNG_IAP - Disables Samsung IAP.

iOS:
	IOS_DISABLE_APPLE_ADS - Disables Apple iAd Ads.
	IOS_DISABLE_GOOGLE_ADS - Disables Google AdMob Ads.
	IOS_DISABLE_APPLE_SCORES - Disables Apple Leaderboards and Acheivements (Game Center).
	IOS_DISABLE_APPLE_IAP - Disables Apple IAP.

WinRT (Win8.1, WP8.1, Win10, WP10):
	WINRT_DISABLE_MS_ADS - Disables Microsoft Ads.
	WINRT_DISABLE_GOOGLE_ADS - Disables Google Ads. (NOTE: you may also want to remove the "Plugins/WP8/GoogleAds.dll")
	WINRT_DISABLE_ADDUPLEX_ADS - Disables AdDuplex Ads.
	WINRT_DISABLE_MS_IAP - Disables Microsoft IAP.

Other platforms don't need any of this as disabling features don't actually effect anything, can't be controlled or don't increase app size.

----------------------------
Support
----------------------------
For questions you can use the forums:
http://reign-studios.net/forums/

For comments, complaints or suggestions you can also email:
support@reign-studios.com

For feature requests email:
features@reign-studios.com