using UnityEngine;
using System.Collections;

public class GameReset : MonoBehaviour {

	WinChecker _WinChecker;

	void Awake() {

		_WinChecker = GameObject.FindObjectOfType<WinChecker>();
	}

	void OnTriggerEnter(Collider colliderObject) {

		if (!_WinChecker.Winning) {
			if (colliderObject.tag == "Ball") {
				Debug.Log("Resetting");
				BallLauncher ballLauncher = colliderObject.transform.parent.GetComponent<BallLauncher>();
				Rigidbody ballRigidBody = colliderObject.GetComponent<Rigidbody>();
				BallBehaviour ballBehaviour = colliderObject.GetComponent<BallBehaviour>();

				ballBehaviour.FiveBounceTrail.GetComponent<ParticleSystem>().Stop();
				ballBehaviour.FiveBounceTrail.GetComponent<ParticleSystem>().Clear();
				ballBehaviour.TenBounceTrail.GetComponent<ParticleSystem>().Stop();
				ballBehaviour.TenBounceTrail.GetComponent<ParticleSystem>().Clear();
				colliderObject.transform.position = ballLauncher.StartPosition;
				colliderObject.transform.rotation = ballLauncher.StartRotation;
				ballLauncher.LaunchPowerMeter = 0f;
				ballRigidBody.isKinematic = true;
				ballRigidBody.velocity = Vector3.zero;
				ballBehaviour.TmpScore = 0f;
				ballBehaviour.TmpBounces = 0;
				ballBehaviour.StartCountingScore = false;
				ballBehaviour.UpdateScoreText();
				StartCoroutine(ResetBallRoutine(colliderObject.gameObject, ballLauncher));
			}
		} else {
			colliderObject.GetComponent<MeshRenderer>().enabled = false;
			colliderObject.GetComponent<Rigidbody>().isKinematic = true;
		}
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
