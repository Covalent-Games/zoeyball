using UnityEngine;
using System.Collections;
using GameData;

public class GameReset : MonoBehaviour {

	WinChecker _WinChecker;
	static GameReset m_Instance;

	void Awake() {

		m_Instance = this;
		_WinChecker = GameObject.FindObjectOfType<WinChecker>();
	}

	void OnTriggerEnter(Collider colliderObject) {

		if (!_WinChecker.Winning) {
			if (colliderObject.tag == "Ball") {
				ResetLevel(colliderObject.gameObject);
			}
		} else {
			colliderObject.GetComponent<MeshRenderer>().enabled = false;
			colliderObject.GetComponent<Rigidbody>().isKinematic = true;
		}
	}

	public static void ResetLevel(GameObject ballGo) {

		BallLauncher ballLauncher = ballGo.transform.parent.GetComponent<BallLauncher>();
		Rigidbody ballRigidBody = ballGo.GetComponent<Rigidbody>();
		BallBehaviour ballBehaviour = ballGo.GetComponent<BallBehaviour>();

		if (!DataManager.SaveData.AchievementProg.IThinkYouMissed &&
			ballBehaviour.TmpBounces == 0 &&
			GameManager.CurrentLevel.LevelID > 2) {
			//Social.ReportProgress(AchievementCodes.IThinkYouMissed, 100f, (bool success) => {
			//	if (success) {
			//		DataManager.SaveData.AchievementProg.IThinkYouMissed = true;
			//	}
			//});
			//TODO: Remove this warning once achievements are fully enabled again.
			Debug.LogWarning("I Think You Missed achievement is surpressed.");
		}

		ballBehaviour.FiveBounceTrail.GetComponent<ParticleSystem>().Stop();
		ballBehaviour.FiveBounceTrail.GetComponent<ParticleSystem>().Clear();
		ballBehaviour.TenBounceTrail.GetComponent<ParticleSystem>().Stop();
		ballBehaviour.TenBounceTrail.GetComponent<ParticleSystem>().Clear();
		ballGo.transform.position = ballLauncher.StartPosition;
		ballGo.transform.rotation = ballLauncher.StartRotation;
		ballLauncher.LaunchPowerMeter = 0f;
		ballLauncher.LaunchButtonText.text = ballLauncher.DefaultLaunchText;
		ballLauncher.LaunchArrowRenderer.enabled = true;
		ballRigidBody.isKinematic = true;
		ballRigidBody.velocity = Vector3.zero;
		ballBehaviour.TmpScore = 0f;
		ballBehaviour.TmpBounces = 0;
		ballBehaviour.StartCountingScore = false;
		ballBehaviour.UpdateScoreText();
		GameReset.m_Instance.StartCoroutine(GameReset.m_Instance.ResetBallRoutine(ballGo, ballLauncher));
	}

	IEnumerator ResetBallRoutine(GameObject ball, BallLauncher ballLauncher) {

		Vector3 scale = ball.transform.localScale;
		ball.transform.localScale = Vector3.zero;

		while (ball.transform.localScale != scale) {
			ball.transform.localScale = Vector3.MoveTowards(ball.transform.localScale, scale, Time.deltaTime * 5);
			yield return null;
		}
		ballLauncher.CanLaunch = true;
		GameObject WinCanvas = GameObject.Find("YouWinLabelCanvas");
		if (WinCanvas) {
			WinCanvas.GetComponent<Canvas>().enabled = false;
		} else {
			Debug.LogError("Whoops, no canvas to turn off");
		}
	}
}
