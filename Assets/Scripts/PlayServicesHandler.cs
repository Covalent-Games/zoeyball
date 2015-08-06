using UnityEngine;
using UnityEngine.SocialPlatforms;
using System.Collections;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GameData;

public class PlayServicesHandler : MonoBehaviour {

	public static class LeaderBoards {
		public const string UpInTheClouds = "CgkI562Uo_MOEAIQBg";
	}
	public static bool LoggingIn = true;

	public void Activate() {

		PlayGamesPlatform.DebugLogEnabled = true;
		PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
			.EnableSavedGames()
			.Build();
		PlayGamesPlatform.InitializeInstance(config);
		PlayGamesPlatform.Activate();

	}

	public void Authenticate() {

		GameObject playButton = GameObject.Find("PlayButton");
		playButton.SetActive(false);
		LoggingIn = true;
		Social.localUser.Authenticate((bool success) => {
			if (success) {
				Debug.Log("Signed into Google Play!");
				if (playButton) {
					playButton.SetActive(true);
				}
			} else {
				Debug.Log("Not signed into Google Play.");
				if (Application.isEditor) {
					Debug.Log(playButton);
					if (playButton) {
						playButton.SetActive(true);
					}
				}
			}
			LoggingIn = false;
		});

	}

	public void ShowLeaderboard(string leaderboard = null) {

		if (leaderboard != null) {
			Social.ShowLeaderboardUI();
		}
	}

	public void ShowAchievements() {

		Social.ShowAchievementsUI();
	}

	public static void UpdateLeaderBoard(string leaderboardId, int oldScore, int newScore) {

		long score = DataManager.SaveData.LeaderBoardScores[leaderboardId] - oldScore + newScore;
		DataManager.SaveData.LeaderBoardScores[leaderboardId] = score;

		Social.ReportScore(DataManager.SaveData.LeaderBoardScores[leaderboardId],
			leaderboardId, (bool success) => {
				if (success) {
					Debug.Log("Leaderboard updated!");
				} else {
					Debug.Log("Leaderboard failed to update.");
				}
			}
		);
	}
}
