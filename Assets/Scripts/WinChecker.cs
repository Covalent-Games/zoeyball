using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using GameData;

public class WinChecker : MonoBehaviour {

	public bool Winning = false;
	GameObject _Confetti;
	BallBehaviour _Ball;
	GameObject[] _Blocks;
	Canvas _ControlUICanvas;

	public static int CurentLevel = 1;
	public Canvas YouWinLabelCanvas;
	public Text YouWinLabel;
	public List<string> WinMessages;

	void Awake() {

		_Ball = GameObject.FindObjectOfType<BallBehaviour>();
		_Confetti = Resources.Load("ConfettiPopper") as GameObject;
		_Blocks = GameObject.FindGameObjectsWithTag("Block");
		_ControlUICanvas = GameObject.Find("ControlUICanvas").GetComponent<Canvas>();

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
			"Well, look at you!",
			"I'm just...I'm so proud.",
			"Imaginary high-five!",
			"And the award for best ball-launching goes to...",
			"Somebody get this winner a cupcake!",
			"Have you been working out?",
			"GOOOOOOOOOOOOOAL!!!",
			"Are you sponsored yet?",
			"That was OK, but I like pizza.",
			"If you could see me, I'd be clapping.",
			"Ding dang doo!",
			"Huh, what happened? Sorry I fell asleep.",
		};
	}

	void OnCollisionEnter(Collision colliderObject) {

		if (colliderObject.gameObject.tag == "Ball") {
			Winning = true;
			_Ball.enabled = false;

			// LevelID is human readable, so is 1 higher than it's index.
			if (GameManager.CurrentLevel.LevelID < DataManager.SaveData.LevelList.Count) {
				var nextLevel = DataManager.SaveData.LevelList[GameManager.CurrentLevel.LevelID];
				nextLevel.IsUnlocked = true;
			}
			if (_Ball.TmpScore > GameManager.CurrentLevel.Score) {
				GameManager.GotHighScore = true;
				GameManager.CurrentLevel.Score = _Ball.TmpScore;
				GameManager.CurrentLevel.Bounces = _Ball.TmpBounces;
				PlayServicesHandler.UpdateLeaderBoard(
					PlayServicesHandler.LeaderBoards.UpInTheClouds,
					(int)GameManager.CurrentLevel.Score);
			}
		}
	}

	void DisplayWinUI() {

		GameManager.Instance.LevelCompleteCanvas.enabled = true;
		GameManager.Instance.DisplayWinDetails();

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
			_ControlUICanvas.enabled = false;
			Invoke("DisplayWinUI", 1f);
		}
	}

}
