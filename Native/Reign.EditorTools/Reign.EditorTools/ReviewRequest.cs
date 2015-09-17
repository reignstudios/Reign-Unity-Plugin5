using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using System;

namespace Reign.EditorTools
{
	public static class ReviewRequest
	{
		[MenuItem("Edit/Reign/Reivew Plugin")]
		static void RequestReview()
		{
			string text =
@"Reviews help us bring in new users and keep support strong.

If you bought the plugin from the Unity Asset Store,
please consider giving a review if you find the plugin features useful.";
			
			if (EditorUtility.DisplayDialog("Plugin Review", text, "Ok Review!", "No Thanks"))
			{
				Application.OpenURL("https://www.assetstore.unity3d.com/en/#!/publisher/4216");
			}

			using (var stream = new FileStream(Application.dataPath+"/Editor/Reign/ReviewSettings", FileMode.Create, FileAccess.Write, FileShare.None))
			using (var reader = new StreamWriter(stream))
			{
				reader.Write("-1");
			}
		}

		[PostProcessBuild]
		static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
		{
			int canReview = 0;
			const int showReviewAtCount = 5;

			try
			{
				using (var stream = new FileStream(Application.dataPath+"/Editor/Reign/ReviewSettings", FileMode.Open, FileAccess.Read, FileShare.None))
				using (var reader = new StreamReader(stream))
				{
					canReview = int.Parse(reader.ReadLine());
				}

				if (canReview != -1)
				using (var stream = new FileStream(Application.dataPath+"/Editor/Reign/ReviewSettings", FileMode.Create, FileAccess.Write, FileShare.None))
				using (var reader = new StreamWriter(stream))
				{
					if (canReview == showReviewAtCount) reader.Write("0");
					else reader.Write((canReview + 1).ToString());
				}
			}
			catch (Exception e)
			{
				Debug.LogError("Reign Review Error: " + e.Message);
			}

			if (canReview == showReviewAtCount) RequestReview();
		}
	}
}
