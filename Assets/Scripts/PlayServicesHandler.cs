using UnityEngine;
using UnityEngine.SocialPlatforms;
using System.Collections;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

public class PlayServicesHandler : MonoBehaviour {

	public static class LeaderBoards {
		public const string UpInTheClouds = "CgkI562Uo_MOEAIQBg";
	}

	public void Activate() {

		Debug.Log(LeaderBoards.UpInTheClouds);
		PlayGamesPlatform.DebugLogEnabled = true;
		PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
			.EnableSavedGames()
			.Build();
		PlayGamesPlatform.InitializeInstance(config);
		PlayGamesPlatform.Activate();

	}

	public bool Authenticate() {

		bool returnValue = true;

		Social.localUser.Authenticate((bool success) => {
			if (success) {
				Debug.Log("Signed into Google Play!");
				returnValue = success;
			} else {
				Debug.Log("Well dang... nothin'");
				returnValue = success;
			}
		});

		return returnValue;
	}

	public void ShowLeaderboard(string leaderboard = null) {

		if (leaderboard != null) {
			Social.ShowLeaderboardUI();
		}
	}

	public static void UpdateLeaderBoard(string id, int score) {

		Social.ReportScore((long)score, id, (bool success) => {
			if (success) {
				Debug.Log("Leaderboard updated!");
			} else {
				Debug.Log("Nope...");
			}
		});
	}
}
