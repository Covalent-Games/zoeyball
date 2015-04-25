using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class WinChecker : MonoBehaviour {

	public bool Winning = false;
	GameManager _GameManger;
	GameObject _Confetti;
	BallBehaviour _Ball;
	GameObject[] _Blocks;

	public static int CurentLevel = 1;
	public Canvas YouWinLabelCanvas;
	public Text YouWinLabel;
	public List<string> WinMessages;

	void Awake() {

		_Ball = GameObject.FindObjectOfType<BallBehaviour>();
		_Confetti = Resources.Load("ConfettiPopper") as GameObject;
		_GameManger = GameObject.FindObjectOfType<GameManager>();
		_Blocks = GameObject.FindGameObjectsWithTag("Block");

	}

	void Start() {

		AssignWinMessages();
	}

	void AssignWinMessages() {

		WinMessages = new List<string>() {
			"Oh nice!",
			"Wow, you did it!",
			"Winner winner chicken dinner!",
			"I knew you had it in ya.",
			"Here's a cookie... ",
			"Congratulations!",
			"Pff... that one was easy!",
			"Level up!",
			"Nice, but can you stand on one leg?",
			"Gee golly! You did it!",
			"I was rooting for you the whole time.",
			"Luck. That's all that was.",
			"Did you see that!?",
			"WHAAAAAAAAAAAAA-",
			"You're unstoppable!!!",
		};
	}

	void OnCollisionEnter(Collision colliderObject) {

		if (colliderObject.gameObject.tag == "Ball") {
			Winning = true;
			_Ball.enabled = false;

			// LevelID is human readable, so is 1 higher than it's index.
			if (GameManager.CurrentLevel.LevelID < DataManager.LevelList.Count) {
				var nextLevel = DataManager.LevelList[GameManager.CurrentLevel.LevelID];
				nextLevel.IsUnlocked = true;
			}
			if (_Ball.TmpScore > GameManager.CurrentLevel.Score) {
				GameManager.CurrentLevel.Score = _Ball.TmpScore;
				GameManager.CurrentLevel.Bounces = _Ball.TmpBounces;
			}
		}
	}

	void Update() {

		if (Winning) {
			int index = Random.Range(0, WinMessages.Count);
			YouWinLabel.text = WinMessages[index];
			YouWinLabelCanvas.enabled = true;
			enabled = false;
			foreach (GameObject block in _Blocks) {
				GameObject.Instantiate(_Confetti, block.transform.position, Quaternion.identity);
				Destroy(block);
			}

			_GameManger.LevelCompleteCanvas.enabled = true;
			_GameManger.DisplayWinDetails();
			//_GameManger.ReturnToLevelPicker(Winning);
		}
	}

}
