using UnityEngine;
using UnityEngine.SocialPlatforms;
using System.Collections;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

public class PlayServicesHandler : MonoBehaviour {

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
}
