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
				PlayServicesHandler.UpdateLeaderBoard(
					PlayServicesHandler.LeaderBoards.UpInTheClouds,
					Mathf.RoundToInt(GameManager.CurrentLevel.Score),
					Mathf.RoundToInt(_Ball.TmpScore));
				GameManager.GotHighScore = true;
				GameManager.CurrentLevel.Score = _Ball.TmpScore;
				GameManager.CurrentLevel.Bounces = _Ball.TmpBounces;
			}

			CheckAchievements(colliderObject);
		}
	}

	void CheckAchievements(Collision colliderObject) {

		if (!DataManager.SaveData.AchievementProg.Pentabounce && _Ball.TmpBounces >= 5) {
			Social.ReportProgress(AchievementCodes.Pentabounce, 100f, (bool success) => {
				DataManager.SaveData.AchievementProg.Pentabounce = true;
			});
		}
		if (!DataManager.SaveData.AchievementProg.TheDecabounce && _Ball.TmpBounces >= 10) {
			Social.ReportProgress(AchievementCodes.TheDecabounce, 100f, (bool success) => {
				DataManager.SaveData.AchievementProg.TheDecabounce = true;
			});
		}
		if (!DataManager.SaveData.AchievementProg.AScoreOfBounces && _Ball.TmpBounces >= 20) {
			Social.ReportProgress(AchievementCodes.AScoreOfBounces, 100f, (bool success) => {
				DataManager.SaveData.AchievementProg.AScoreOfBounces = true;
			});
		}
		if (!DataManager.SaveData.AchievementProg.Check && GameManager.CurrentLevel.LevelID == 20) {
			Social.ReportProgress(AchievementCodes.Check, 100f, (bool success) => {
				DataManager.SaveData.AchievementProg.Check = true;
			});
		}
		if (!DataManager.SaveData.AchievementProg.Champ && _Ball.TmpScore >= 100) {
			Social.ReportProgress(AchievementCodes.Champ, 100f, (bool success) => {
				DataManager.SaveData.AchievementProg.Champ = true;
			});
		}
		if (!DataManager.SaveData.AchievementProg.Olympian && _Ball.TmpScore >= 200) {
			Social.ReportProgress(AchievementCodes.Olympian, 100f, (bool success) => {
				DataManager.SaveData.AchievementProg.Olympian = true;
			});
		}
		if (!DataManager.SaveData.AchievementProg.YoureRidiculous && _Ball.TmpScore >= 500) {
			Social.ReportProgress(AchievementCodes.YoureRidiculous, 100f, (bool success) => {
				DataManager.SaveData.AchievementProg.YoureRidiculous = true;
			});
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
