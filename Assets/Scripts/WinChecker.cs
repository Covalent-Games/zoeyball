using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using GameData;

public class WinChecker : MonoBehaviour {

	public bool Winning = false;
	public AudioClip WinClapAudio;
	GameObject _Confetti;
	BallBehaviour _Ball;
	GameObject[] _Blocks;
	Canvas _ControlUICanvas;

	public static int CurentLevel = 1;
	public Canvas YouWinLabelCanvas;
	public Text YouWinLabel;
	public List<string> WinMessages;

	void Awake() {

		_Confetti = Resources.Load("ConfettiPopper") as GameObject;
		_Blocks = GameObject.FindGameObjectsWithTag("Block");
		_ControlUICanvas = GameObject.Find("ControlUICanvas").GetComponent<Canvas>();

	}

	void Start() {

		_Ball = BallLauncher.Ball.GetComponent<BallBehaviour>();
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
			"I really didn't think you were going to make it.",
			"... so then I said, 'It's number 12!' Bahahah! Good one, right?",
			"Woah, close call.",
			"Did Han really shoot first?",
			"That was awesome!",
			"What!? I said what!? WHHAAT!? But really, well done.",
			"That was a risky move, but it paid off.",
			"You're good at this",
			"Don't forget to check the Leaderboards!",
			"Have you unlocked every Achievement yet?",
			"I wish junk food was gross tasting.",
			"You're killin' it out there!",
			"This is impressive to watch.",
			"Oh- Oh wow. Amazing.",
			"Did that just happen?",
			"I think you can do better...",
			"Well, you beat *my* score!",
			"You should try that one again, though.",
			"It's super effective!",
			"Yes, I believe the answer is 19?",
			"Sure, I'll clap.",
			"Pneumonoultramicroscopicsilicovolcanoconiosis. My uncle had it.",
			"The day just keeps getting better.",
			"Thanks for playing! Come back and see us again soon!",
			"Nailed it. Am I right, Or am I right?",
			"It's Madison's fault I keep saying all this weird stuff.",
		};
	}

	void OnCollisionEnter(Collision colliderObject) {

		if (colliderObject.gameObject.tag == "Ball") {
			Winning = true;
			_Ball.enabled = false;
			_Ball.TenBounceParticle.GetComponent<ParticleSystem>().Play();

			if (GameManager.Instance != null) {
				GameManager.Instance.CheckAchievements(colliderObject);
			} else {
				Debug.Log("GameManger.Instance is null, for whatever reason...");
			}
			//CheckAchievements(colliderObject);
		}
	}

	void CheckAchievements(Collision colliderObject) {

		if (!DataManager.SaveData.AchievementProg.Pentabounce && _Ball.CurrentBounces >= 5) {
			Social.ReportProgress(AchievementCodes.Pentabounce, 100f, (bool success) => {
				DataManager.SaveData.AchievementProg.Pentabounce = true;
			});
		}
		if (!DataManager.SaveData.AchievementProg.TheDecabounce && _Ball.CurrentBounces >= 10) {
			Social.ReportProgress(AchievementCodes.TheDecabounce, 100f, (bool success) => {
				DataManager.SaveData.AchievementProg.TheDecabounce = true;
			});
			DataManager.SaveData.UnlockedBalls.Add("BallBaseBall", true);
			PlayerPrefs.SetInt("BallBaseBall", 1);
		}
		if (!DataManager.SaveData.AchievementProg.AScoreOfBounces && _Ball.CurrentBounces >= 20) {
			Social.ReportProgress(AchievementCodes.AScoreOfBounces, 100f, (bool success) => {
				DataManager.SaveData.AchievementProg.AScoreOfBounces = true;
			});
		}
		if (!DataManager.SaveData.AchievementProg.Check && GameManager.CurrentLevel.LevelID == 20) {
			Social.ReportProgress(AchievementCodes.Check, 100f, (bool success) => {
				DataManager.SaveData.AchievementProg.Check = true;
			});
			DataManager.SaveData.UnlockedBalls.Add("BallEarth", true);
			PlayerPrefs.SetInt("BallEarth", 1);
		}
		if (!DataManager.SaveData.AchievementProg.Champ && _Ball.CurrentScore >= 100) {
			Social.ReportProgress(AchievementCodes.Champ, 100f, (bool success) => {
				DataManager.SaveData.AchievementProg.Champ = true;
			});
			DataManager.SaveData.UnlockedBalls.Add("BallPokeball", true);
			PlayerPrefs.SetInt("Ball8Ball", 1);
		}
		if (!DataManager.SaveData.AchievementProg.Olympian && _Ball.CurrentScore >= 200) {
			Social.ReportProgress(AchievementCodes.Olympian, 100f, (bool success) => {
				DataManager.SaveData.AchievementProg.Olympian = true;
			});
		}
		if (!DataManager.SaveData.AchievementProg.YoureRidiculous && _Ball.CurrentScore >= 500) {
			Social.ReportProgress(AchievementCodes.YoureRidiculous, 100f, (bool success) => {
				DataManager.SaveData.AchievementProg.YoureRidiculous = true;
			});
		}
	}

	void DisplayWinUI() {

		GameManager.Instance.LevelCompleteCanvas.enabled = true;
		if (GameManager.GotHighScore || _Ball.CurrentBounces >= GameManager.CurrentLevel.BounceGoal) {
			//TODO: Since all this method does is enable elements, it should just be here instead.
			GameManager.Instance.HighScoreStamp.transform.parent.gameObject.SetActive(true);
			GameManager.Instance.DisplayWinDetails();
		} else {
			// Moves the accolade container up one step in the hierarchy.
			GameManager.Instance.HighScoreStamp.transform.parent.gameObject.SetActive(false);
		}
		if (!(PlayerPrefs.HasKey("Sound") && PlayerPrefs.GetString("Sound") == "Off")) {
			AudioSource.PlayClipAtPoint(WinClapAudio, Camera.main.transform.position, .5f);
		}

	}

	void UpdateCloudData() {

		GameManager.DataWrangler.StartSaveGameData();
		// LevelID is human readable, so is 1 higher than it's index.
		if (GameManager.CurrentLevel.LevelID < DataManager.SaveData.LevelList.Count) {
			var nextLevel = DataManager.SaveData.LevelList[GameManager.CurrentLevel.LevelID];
			nextLevel.IsUnlocked = true;
		}
		if (_Ball.CurrentScore > GameManager.CurrentLevel.Score) {
			PlayServicesHandler.UpdateLeaderBoard(
				PlayServicesHandler.LeaderBoards.UpInTheClouds,
				Mathf.RoundToInt(GameManager.CurrentLevel.Score),
				Mathf.RoundToInt(_Ball.CurrentScore));
			GameManager.GotHighScore = true;
			GameManager.CurrentLevel.Score = _Ball.CurrentScore;
			GameManager.CurrentLevel.Bounces = _Ball.CurrentBounces;
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
			_ControlUICanvas.enabled = false;
			Invoke("UpdateCloudData", .95f);
			Invoke("DisplayWinUI", 1f);
		}
	}
}
