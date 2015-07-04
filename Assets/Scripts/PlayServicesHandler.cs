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

	public void Activate() {

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
				Debug.Log("Not signed into Google Play.");
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
