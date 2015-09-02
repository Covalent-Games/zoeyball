using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using GameData;

public class GameReset : MonoBehaviour {

	private WinChecker _winChecker;
	private static GameReset _instance;
	private Canvas _winCanvas;

	void Awake() {

		_instance = this;
		_winChecker = GameObject.FindObjectOfType<WinChecker>();
		_winCanvas = GameObject.Find("YouWinLabelCanvas").GetComponent<Canvas>();
	}

	void OnTriggerEnter(Collider colliderObject) {

		if (!_winChecker.Winning) {
			if (colliderObject.tag == "Ball") {
				ResetLevel(colliderObject.gameObject.GetComponent<BallBehaviour>());
			}
		} else {
			colliderObject.GetComponent<MeshRenderer>().enabled = false;
			colliderObject.GetComponent<Rigidbody>().isKinematic = true;
		}
	}

	public static void ResetLevel(BallBehaviour ballBehaviour) {

#if !UNITY_EDITOR
		if (!DataManager.SaveData.AchievementProg.IThinkYouMissed &&
			ballBehaviour.CurrentBounces == 0 &&
			GameManager.CurrentLevel.LevelID > 2) {
			//Social.ReportProgress(AchievementCodes.IThinkYouMissed, 100f, (bool success) => {
			//	if (success) {
			//		DataManager.SaveData.AchievementProg.IThinkYouMissed = true;
			//	}
			//});
			//TODO: Remove this warning once achievements are fully enabled again.
			Debug.LogWarning("'I Think You Missed' achievement is surpressed.");
		}
#endif

		//TODO: This is silly and should be moved to methods within the referenced objects.
		ballBehaviour.Reset();
		ballBehaviour.LaunchPlatform.Reset();
		GameReset._instance.StartCoroutine(GameReset._instance.ResetBallRoutine(ballBehaviour));
	}

	IEnumerator ResetBallRoutine(BallBehaviour ball) {

		Vector3 scale = ball.transform.localScale;
		ball.transform.localScale = Vector3.zero;

		while (ball.transform.localScale != scale) {
			ball.transform.localScale = Vector3.MoveTowards(ball.transform.localScale, scale, Time.deltaTime * 5);
			yield return null;
		}
		ball.LaunchPlatform.CanLaunch = true;
		_winCanvas.enabled = false;
	}
}
