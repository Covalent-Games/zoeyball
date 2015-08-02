using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TransformExtensions;
using System.Collections;
using System.Collections.Generic;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using System;
using GameData;
using UnityEngine.Audio;

[RequireComponent(typeof(PlayServicesHandler))]
public class GameManager : MonoBehaviour {

	#region Members
	static GameManager _Instance;
	public static GameManager Instance {
		get {
			if (_Instance == null) {
				_Instance = GameObject.FindObjectOfType<GameManager>();
			}
			return _Instance;
		}
		private set {
			_Instance = value;
		}
	}
	public static Level CurrentLevel;
	public static GameObject SelectedBall;
	public PlayServicesHandler GooglePlay;
	public TextAsset LevelData;
	public Canvas LevelCompleteCanvas;
	public Canvas LoadingScreenCanvas;
	public Canvas AchievementCanvas;
	public Image LevelSelectBkGrndImage;
	public GameObject HighScoreStamp;
	public GameObject BounceGoalStamp;
	public Animator MenuAnimator;
	public static DataManager DataWrangler;
	public GameObject LevelButtonResource;
	public Text LevelText;
	public static bool GotHighScore = false;
	public bool IsBusy = false;

	private GameObject LevelDisplayCanvasGO;
	private Text WorldName;
	private int SelectedWorldID = 0;
	private GameObject LevelDisplayContent;
	private AudioManager AudioWrangler;
	private BallBehaviour _BallBehavior;
	private BallLauncher _BallLauncher;
	private float SkyBoxRotation = 0f;

	private bool MenuOpen = false;
	#endregion

	#region Initializers
	void Awake() {

		GameManager.Instance = this;
		GooglePlay = GetComponent<PlayServicesHandler>();
		DataWrangler = new DataManager();
		DataWrangler.LoadLevelTemplate(LevelData);
		AudioWrangler = GetComponent<AudioManager>();
		LoadingScreenCanvas = transform.FindChildRecursive("LoadingScreenCanvas").GetComponent<Canvas>();
		AchievementCanvas = transform.FindChild("tmpAchievementCanvas").GetComponent<Canvas>();
		MenuAnimator = GameObject.FindGameObjectWithTag("MenuPanel").GetComponent<Animator>();
		MenuAnimator.enabled = false;
		LevelSelectBkGrndImage = GameObject.Find("LevelSelectBackground").GetComponent<Image>();
		SelectedBall = (GameObject)Resources.Load("Balls/" + (PlayerPrefs.HasKey("CurrentBall") ? PlayerPrefs.GetString("CurrentBall") : "BallYellow"));
		DontDestroyOnLoad(gameObject);

		AchievementCodes.ADict = new Dictionary<string, string>(){
			{"CgkI562Uo_MOEAIQAQ", "Check"},
			{"CgkI562Uo_MOEAIQBw", "I Think You Missed"},
			{"CgkI562Uo_MOEAIQAg", "Pentabounce"},
			{"CgkI562Uo_MOEAIQBQ", "The Decabounce"},
			{"CgkI562Uo_MOEAIQBA", "A Score of Bounces"},
			{"CgkI562Uo_MOEAIQCA", "Champ"},
			{"CgkI562Uo_MOEAIQCQ", "Olympian"},
			{"CgkI562Uo_MOEAIQCg", "You're Ridiculous"},
		};
	}

	void Start() {

		DataWrangler.OnDataChangeStarted += MarkAsBusy;
		DataWrangler.OnDataLoaded += MarkAsNotBusy;
		DataWrangler.OnDataSaved += MarkAsNotBusy;
		Time.timeScale = 1.15f;
		GooglePlay.Activate();
		GooglePlay.Authenticate();
	}

	#endregion

	#region Polled methods
	void Update() {

		SkyBoxRotation += 2 * Time.deltaTime;
		SkyBoxRotation %= 360;
		RenderSettings.skybox.SetFloat("_Rotation", SkyBoxRotation);

		if (Input.GetKeyDown(KeyCode.Escape)) {
			// Start screen and level select
			if (Application.loadedLevel == 0 || Application.loadedLevel == 1) {
				Quit();
			}
			// Ball selection
			else if (Application.loadedLevel == 2) {
				ReturnButton();
			} 
			// Level
			else {
				ReturnButton();
			}
		}
	}
	#endregion

	#region Button Handlers
	public void Play() {

		if (IsBusy) { return; }

		try {
			//MarkAsBusy();
			DataWrangler.OnDataLoaded += LoadLevelPicker;
			DataWrangler.StartLoadGameData();
			DataWrangler.StartRecordingPlayTime();
		} catch (NullReferenceException e) {
			Debug.Log("*******GameManager.Play exception: " + e.Message);
			Debug.Log(e.StackTrace);
			Debug.Log("*******GameManager.Play innerexception: " + e.InnerException);
			if (e.Data.Count > 0) {
				Debug.Log("EXCEPTION DATA:\n");
				foreach (DictionaryEntry pair in e.Data) {
					Debug.Log(string.Format("Key: {0}\nData: {1}", pair.Key.ToString(), pair.Value));
				}
			}

			if (Application.isEditor) {
				Debug.LogWarning(
					"Loading LevelPicker. You're running in the Editor so no save files used!");
				if (MenuOpen) {
					ToggleMenu();
				}
				LoadLevelPicker();
				return;
			}
		}
		if (MenuOpen) {
			ToggleMenu();
		}
	}

	public void LoadBallSelection() {

		Application.LoadLevel("BallSelection");
	}

	public void ToggleMenu() {

		MenuAnimator.enabled = true;
		if (MenuOpen) {
			MenuAnimator.Play("MenuSlideOut");
		} else {
			MenuAnimator.Play("MenuSlideIn");
		}
		MenuOpen = !MenuOpen;

	}

	public void Quit() {

		DataWrangler.OnDataSaved += Application.Quit;
		// Close the menu
		if (MenuOpen) {
			ToggleMenu();
		}
		DataWrangler.StartSaveGameData();
	}

	public void ReturnButton() {

		// If we're already in the level selection screen this button does nothing.
		if (Application.loadedLevel == 0 || Application.loadedLevel == 1) {
			return;
		}
		LevelCompleteCanvas.enabled = false;
		// Close the menu
		if (MenuOpen)
			ToggleMenu();
		ReturnToLevelPicker(true, true);
	}

	public void Restart(bool toggleMenu = false) {

		// If we're already in the level selection screen this button does nothing.
		if (Application.loadedLevel == 0 || Application.loadedLevel == 1) {
			return;
		}

		RestartCache.Score = _BallBehavior.TmpScore;
		RestartCache.Bounces = _BallBehavior.TmpBounces;
		RestartCache.StartBlockRotation = _BallLauncher.transform.rotation;
		RestartCache.Powerbar = _BallLauncher.LaunchPowerMeter;
		RestartCache.LoadFromCache = true;

		_BallBehavior.FiveBounceTrail.GetComponent<ParticleSystem>().Stop();
		_BallBehavior.FiveBounceTrail.GetComponent<ParticleSystem>().Clear();
		_BallBehavior.TenBounceTrail.GetComponent<ParticleSystem>().Stop();
		_BallBehavior.TenBounceTrail.GetComponent<ParticleSystem>().Clear();
		LevelCompleteCanvas.enabled = false;
		HighScoreStamp.GetComponent<Image>().enabled = false;

		if (toggleMenu) {
			ToggleMenu();
		}
		Application.LoadLevel(Application.loadedLevel);
	}

	public void NextButton() {

		RestartCache.LoadFromCache = false;

		LevelCompleteCanvas.enabled = false;
		HighScoreStamp.GetComponent<Image>().enabled = false;
		// If next level exists, load it.
		if (DataManager.SaveData.LevelList[CurrentLevel.LevelID] != null)
			LoadLevelByID(CurrentLevel.LevelID + 1);
		else
			ReturnToLevelPicker(true, true);
	}

	public void NextWorldButton() {

		SelectedWorldID++;
		if (DataManager.SaveData.LevelList[-1].WorldID < SelectedWorldID) {
			SelectedWorldID--;
		} else {
			//Do world switching stuff.
		}
	}
	#endregion

	#region Misc
	void LoadLevelPicker() {

		Application.LoadLevel("LevelPicker");
	}

	public void ReturnToLevelPicker(bool save, bool immediate = false) {

		RestartCache.LoadFromCache = false;

		if (save) {
			DataWrangler.StartSaveGameData();
		}
		if (immediate) {
			Application.LoadLevel("LevelPicker");
		} else {
			StartCoroutine(ReturnToLevelPickerRoutine());
		}
	}

	IEnumerator ReturnToLevelPickerRoutine() {

		yield return new WaitForSeconds(2.5f);
		Application.LoadLevel("LevelPicker");
	}

	public static void LoadLevelByID(int ID) {

#if UNITY_EDITOR
		Application.LoadLevel(string.Format("Level{0}", ID));
		GameManager.CurrentLevel = DataManager.SaveData.LevelList[ID - 1];

#else

		if (ID != 1 && !DataManager.SaveData.LevelList[ID - 1].IsUnlocked) { return; }

		Application.LoadLevel(string.Format("Level{0}", ID));
		GameManager.CurrentLevel = DataManager.SaveData.LevelList[ID - 1];

#endif

	}

	void ApplyCacheToLoadedLevel() {

		_BallLauncher.transform.rotation = RestartCache.StartBlockRotation;
		_BallBehavior.TmpScore = RestartCache.Score;
		_BallBehavior.TmpBounces = RestartCache.Bounces;
		_BallBehavior.UpdateScoreText();
		_BallBehavior.TmpScore = 0;
		_BallBehavior.TmpBounces = 0;
		_BallLauncher.LanchMeterImage.fillAmount = RestartCache.Powerbar / _BallLauncher.MaxLaunchPower;
	}

	void PopulateLevelListUI() {

		if (!LevelDisplayContent) {
			Debug.LogError("Content panel missing or renamed.");
		}
		Text levelName;
		Image padlock;
		Text scoreText;
		Text bounceText;
		Image crown;

		GameObject button;
		foreach (var level in DataManager.SaveData.LevelList) {
			if (level.WorldID < SelectedWorldID) { continue; }
			if (level.WorldID > SelectedWorldID) { break; }
			//TOOD: Have this not set every time.
			WorldName.text = World.Names[level.WorldID];
			button = Instantiate(LevelButtonResource);
			button.transform.SetParent(LevelDisplayContent.transform, false);

			button.GetComponent<LoadLevelButton>().LevelToLoad = level;

			levelName = button.transform.FindChild("LevelName").GetComponent<Text>();
			padlock = button.transform.FindChild("Padlock").GetComponent<Image>();
			scoreText = button.transform.FindChild("ScoreText").GetComponent<Text>();
			bounceText = button.transform.FindChild("BounceText").GetComponent<Text>();
			crown = button.transform.FindChild("Crown").GetComponent<Image>();

			levelName.text = string.Format("Level {0}", level.LevelID);
			if (!level.IsUnlocked && level.LevelID != 1) {
				button.transform.FindChild("LevelName").GetComponent<Text>().color = Color.black;
				padlock.enabled = true;
				button.transform.FindChild("Overlay").GetComponent<Image>().enabled = true;
			} else {
				scoreText.text = Mathf.RoundToInt(level.Score).ToString();
				if (level.BounceGoalUnlocked) {
					bounceText.text = string.Format("{0} / {1}", level.Bounces.ToString(), level.BounceGoal);
					crown.enabled = true;
				} else if (level.Score == 0) {
					bounceText.text = level.Bounces.ToString();
					crown.enabled = false;
				} else {
					bounceText.text = string.Format("{0} / {1}", level.Bounces.ToString(), level.BounceGoal);
					crown.enabled = false;
				}
			}
		}
	}

	public void DisplayWinDetails() {

		if (GotHighScore) {
			HighScoreStamp.GetComponent<Image>().enabled = true;
			HighScoreStamp.GetComponent<Animation>().Play();
			//HighScoreStamp.transform.FindChild("HighScoreParticles")
			//	.GetComponent<ParticleSystem>().Play();
			GotHighScore = false;
		}
		if (_BallBehavior.TmpBounces >= CurrentLevel.BounceGoal) {
			Debug.Log(_BallBehavior.TmpBounces);
			Debug.Log(CurrentLevel.BounceGoal);
			BounceGoalStamp.transform.parent = HighScoreStamp.transform.parent;
			CurrentLevel.BounceGoalUnlocked = true;
			BounceGoalStamp.GetComponent<Image>().enabled = true;
			BounceGoalStamp.GetComponent<Animation>().Play();
		} else {
			BounceGoalStamp.SetActive(false);
			BounceGoalStamp.transform.parent = HighScoreStamp.transform.parent.parent;
		}

	}

	public void MarkAsBusy() {

		IsBusy = true;
		Debug.Log("-------GameManager busy---------");
		this.LoadingScreenCanvas.enabled = true;
	}

	public void MarkAsNotBusy() {

		IsBusy = false;
		Debug.Log("--------GameManager Not Busy----------");
		this.LoadingScreenCanvas.enabled = false;
	}

	public void CheckAchievements(Collision colliderObject) {

		BallBehaviour ball = colliderObject.gameObject.GetComponent<BallBehaviour>();
		if (!DataManager.SaveData.AchievementProg.Pentabounce && ball.TmpBounces >= 5) {
			//Social.ReportProgress(AchievementCodes.Pentabounce, 100f, (bool success) => {
			//	DataManager.SaveData.AchievementProg.Pentabounce = true;
			//});
			ShowAchievementPanel_tmp(AchievementCodes.ADict[AchievementCodes.Pentabounce]);
		}
		if (!DataManager.SaveData.AchievementProg.TheDecabounce && ball.TmpBounces >= 10) {
			//Social.ReportProgress(AchievementCodes.TheDecabounce, 100f, (bool success) => {
			//	DataManager.SaveData.AchievementProg.TheDecabounce = true;
			//});
			ShowAchievementPanel_tmp(AchievementCodes.ADict[AchievementCodes.TheDecabounce]);
			//DataManager.SaveData.UnlockedBalls.Add("BallBaseBall", true);
			//PlayerPrefs.SetInt("BallBaseBall", 1);
		}
		if (!DataManager.SaveData.AchievementProg.AScoreOfBounces && ball.TmpBounces >= 20) {
			//Social.ReportProgress(AchievementCodes.AScoreOfBounces, 100f, (bool success) => {
			//	DataManager.SaveData.AchievementProg.AScoreOfBounces = true;
			//});
			ShowAchievementPanel_tmp(AchievementCodes.ADict[AchievementCodes.AScoreOfBounces]);
		}
		if (!DataManager.SaveData.AchievementProg.Check && GameManager.CurrentLevel.LevelID == 20) {
			//Social.ReportProgress(AchievementCodes.Check, 100f, (bool success) => {
			//	DataManager.SaveData.AchievementProg.Check = true;
			//});
			ShowAchievementPanel_tmp(AchievementCodes.ADict[AchievementCodes.Check]);
			//DataManager.SaveData.UnlockedBalls.Add("BallEarth", true);
			//PlayerPrefs.SetInt("BallEarth", 1);
		}
		if (!DataManager.SaveData.AchievementProg.Champ && ball.TmpScore >= 100) {
			//Social.ReportProgress(AchievementCodes.Champ, 100f, (bool success) => {
			//	DataManager.SaveData.AchievementProg.Champ = true;
			//});
			ShowAchievementPanel_tmp(AchievementCodes.ADict[AchievementCodes.Champ]);
			//DataManager.SaveData.UnlockedBalls.Add("BallPokeball", true);
			//PlayerPrefs.SetInt("Ball8Ball", 1);
		}
		if (!DataManager.SaveData.AchievementProg.Olympian && ball.TmpScore >= 200) {
			//Social.ReportProgress(AchievementCodes.ADict[AchievementCodes.Olympian, 100f, (bool success) => {
			//	DataManager.SaveData.AchievementProg.Olympian = true;
			//});
			ShowAchievementPanel_tmp(AchievementCodes.ADict[AchievementCodes.Olympian]);
		}
		if (!DataManager.SaveData.AchievementProg.YoureRidiculous && ball.TmpScore >= 500) {
			//Social.ReportProgress(AchievementCodes.YoureRidiculous, 100f, (bool success) => {
			//	DataManager.SaveData.AchievementProg.YoureRidiculous = true;
			//});
			ShowAchievementPanel_tmp(AchievementCodes.ADict[AchievementCodes.YoureRidiculous]);
		}
	}

	void ShowAchievementPanel_tmp(string achievementTitle) {

		AchievementCanvas.transform.FindChildRecursive("AchievementText").GetComponent<Text>().text =
			achievementTitle;
		AchievementCanvas.enabled = true;
		Invoke("DisableAchievementPanel_tmp", 5f);
	}

	void DisableAchievementPanel_tmp() {

		AchievementCanvas.enabled = false;
	}
	#endregion

	#region Event Subs
	void OnApplicationQuit() {

		PlayerPrefs.Save();
	}

	void OnLevelWasLoaded(int levelIndex) {

		if (IsBusy) {
			// In case something went wrong, we don't want the loading screen stuck open.
			MarkAsNotBusy();
		}

		switch (levelIndex) {
			default:
				LevelSelectBkGrndImage.enabled = false;
				AudioWrangler.ChangeMusicVolume(.275f);
				_BallLauncher = GameObject.FindObjectOfType<BallLauncher>();
				// Now that Ball is static this reference isn't needed
				_BallBehavior = BallLauncher.Ball.GetComponent<BallBehaviour>();
				Text LevelText = _BallLauncher.transform.parent.FindChildRecursive("LevelText")
					.GetComponent<Text>();
				LevelText.text = string.Format("Level {0}", CurrentLevel.LevelID);
				if (RestartCache.LoadFromCache) {
					ApplyCacheToLoadedLevel();
				}
				break;
			case 0:
				LevelSelectBkGrndImage.enabled = false;
				AudioWrangler.ChangeMusicVolume(1f);
				break;
			case 1:
				LevelSelectBkGrndImage.enabled = true;
				LevelDisplayCanvasGO = GameObject.Find("LevelDisplayCanvas");
				WorldName = LevelDisplayCanvasGO.transform.FindChildRecursive("WorldName")
					.GetComponent<Text>();
				LevelDisplayContent = LevelDisplayCanvasGO.transform
					.FindChildRecursive("LevelDisplayContent").gameObject;
				PopulateLevelListUI();
				AudioWrangler.ChangeMusicVolume(1f);
				break;
			case 2:
				LevelSelectBkGrndImage.enabled = false;
				break;
		}
	}

	void OnSavedGameSelected(SelectUIStatus status, ISavedGameMetadata metaData) {

		if (status == SelectUIStatus.SavedGameSelected) {
			Debug.Log("Selected the saved game");
		} else {
			Debug.LogError("The save game selection got borked");
		}
	}
	#endregion
}
