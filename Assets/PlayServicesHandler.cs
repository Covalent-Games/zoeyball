using UnityEngine;
using UnityEngine.SocialPlatforms;
using System.Collections;
using GooglePlayGames;

public class PlayServicesHandler : MonoBehaviour {

	public static void Activate() {

		PlayGamesPlatform.DebugLogEnabled = true;
		PlayGamesPlatform.Activate();


	}

	public static bool Authenticate() {

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
}
