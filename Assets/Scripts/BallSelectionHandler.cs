using UnityEngine;
using System.Collections;
using GameData;

public class BallSelectionHandler : MonoBehaviour {

	int CurrentChildIndex = 0;

	void Awake() {

		GameObject ball;
		Vector3 ballPosition = new Vector3(0, 0, 0);
		foreach (var entry in DataManager.BallNamePathPairs) {
			if (DataManager.SaveData.UnlockedBalls[entry.Key]) {
				ball = (GameObject)Instantiate(
					Resources.Load(entry.Value));
				ball.GetComponent<BallBehaviour>().enabled = false;
				ball.transform.position = ballPosition;
				ballPosition.x++;
				ball.transform.parent = transform;
			}
		}
	}

	public void ScrollRight() {

		if (CurrentChildIndex + 1 < transform.childCount) {
			CurrentChildIndex++;
		}
	}

	public void ScrollLeft() {

		if (CurrentChildIndex > 0) {
			CurrentChildIndex--;
		}
	}

	public void SelectBall() {

		GameObject ball = transform.GetChild(CurrentChildIndex).gameObject;
		string ballName = ball.name.Replace("(Clone)", "").Trim();
		GameManager.SelectedBall = (GameObject)Resources.Load(DataManager.BallNamePathPairs[ballName]);

		Instantiate(ball.GetComponent<BallBehaviour>().FiveBounceParticle,
			ball.transform.position,
			Quaternion.identity);
		DataManager.SaveData.CurrentSelectedBallName = ballName;

		Invoke("LoadLevelPicker", 1.5f);

	}

	void LoadLevelPicker() {

		Application.LoadLevel(1);
	}

	void Update() {

		Vector3 moveTo = transform.GetChild(CurrentChildIndex).transform.position;
		moveTo.z = Camera.main.transform.position.z;

		Camera.main.transform.position = Vector3.Slerp(
			Camera.main.transform.position,
			moveTo,
			10 * Time.deltaTime);
	}
}
