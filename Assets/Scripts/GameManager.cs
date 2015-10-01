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
	public LoadingScreenManager LoadingScreen;
	public Canvas AchievementCanvas;
	public Canvas QuitConfirmCanvas;
	public Image LevelSelectBkGrndImage;
	public GameObject HighScoreStamp;
	public GameObject BounceGoalStamp;
	public Animator MenuAnimator;
	public static DataManager DataWrangler;
	public GameObject LevelButtonResource;
	public Text LevelText;
	public static bool GotHighScore = false;
	public bool IsBusy = false;

	private GameObject _levelDisplayCanvasGO;
	private Transform _levelDisplayElementContainer;
	private Text _worldName;
	private int _selectedWorldID = 0;
	private GameObject _levelDisplayContent;
	private AudioManager _audioWrangler;
	private BallBehaviour _ballBehavior;
	private BallLauncher _ballLauncher;
	private float _skyBoxRotation = 0f;

	private bool _menuOpen = false;
	private bool _loadError = false;
	#endregion

	#region Initializers
	void Awake() {

		GameManager.Instance = this;
		GooglePlay = GetComponent<PlayServicesHandler>();
		DataWrangler = new DataManager();
		DataWrangler.LoadLevelTemplate(LevelData);
		_audioWrangler = GetComponent<AudioManager>();
		LoadingScreen = transform.FindChildRecursive("LoadingScreenCanvas").GetComponent<LoadingScreenManager>();
		AchievementCanvas = transform.FindChild("tmpAchievementCanvas").GetComponent<Canvas>();
		MenuAnimator = GameObject.FindGameObjectWithTag("MenuPanel").GetComponent<Animator>();
		_levelDisplayElementContainer = transform.FindChildRecursive("ElementContainer");
		MenuAnimator.enabled = false;
		LevelSelectBkGrndImage = GameObject.Find("LevelSelectBackground").GetComponent<Image>();
		SelectedBall = (GameObject)Resources.Load("Balls/" + (PlayerPrefs.HasKey("CurrentBall") ? PlayerPrefs.GetString("CurrentBall") : "BallYellow"));
		DontDestroyOnLoad(gameObject);

		AchievementCodes.CodeToNameDict = new Dictionary<string, string>(){
			{"CgkI562Uo_MOEAIQAQ", "Check"},
			{"CgkI562Uo_MOEAIQBw", "I Think You Missed"},
			{"CgkI562Uo_MOEAIQAg", "Pentabounce"},
			{"CgkI562Uo_MOEAIQBQ", "The Decabounce"},
			{"CgkI562Uo_MOEAIQBA", "A Score of Bounces"},
			{"CgkI562Uo_MOEAIQCA", "Champ"},
			{"CgkI562Uo_MOEAIQCQ", "Olympian"},
			{"CgkI562Uo_MOEAIQCg", "You're Ridiculous"},
		};

		GooglePlay.Activate();
		GooglePlay.Authenticate();
	}

	void Start() {

		DataWrangler.OnDataChangeStarted += MarkAsBusy;
		DataWrangler.OnDataLoaded += MarkAsNotBusy;
		DataWrangler.OnDataSaved += MarkAsNotBusy;
		Time.timeScale = 1.15f;
	}

	#endregion

	#region Polled methods
	void Update() {

		_skyBoxRotation += 2 * Time.deltaTime;
		_skyBoxRotation %= 360;
		RenderSettings.skybox.SetFloat("_Rotation", _skyBoxRotation);

		if (Input.GetKeyDown(KeyCode.Escape) && !IsBusy) {
			if (Application.loadedLevel == 0 || Application.loadedLevel == 1) {
				QuitConfirmCanvas.GetComponent<Canvas>().enabled = true;
			} else {
				ReturnButton();
			}
		}
	}
	#endregion

	#region Button Handlers
	public void Play() {

		if (IsBusy) { return; }

		try {
			Debug.Log("Play pressed. Starting data loading.");
			DataWrangler.OnDataLoaded += LoadLevelPicker;
			DataWrangler.StartLoadGameData();
			DataWrangler.StartRecordingPlayTime();
		} catch (Exception e) {
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
				if (_menuOpen) {
					ToggleMenu();
				}
				LoadLevelPicker();
				return;
			}
		}
		if (_menuOpen) {
			ToggleMenu();
		}
	}

	void DisplayLoadError() {

		_loadError = true;
	}

	public void LoadBallSelection() {

		Application.LoadLevel("BallSelection");
	}

	public void ToggleMenu() {

		MenuAnimator.enabled = true;
		if (_menuOpen) {
			MenuAnimator.Play("MenuSlideOut");
		} else {
			MenuAnimator.Play("MenuSlideIn");
		}
		_menuOpen = !_menuOpen;

	}

	public void Quit() {

		Application.Quit();
	}

	public void ReturnButton() {

		// If we're already in the level selection screen this button does nothing.
		if (Application.loadedLevel == 0 || Application.loadedLevel == 1) {
			return;
		}
		LevelCompleteCanvas.enabled = false;
		// Close the menu
		if (_menuOpen)
			ToggleMenu();
		ReturnToLevelPicker(true);
	}

	public void Restart(bool toggleMenu = false) {

		// If we're already in the level selection screen this button does nothing.
		if (Application.loadedLevel == 0 || Application.loadedLevel == 1) {
			return;
		}

		RestartCache.Score = _ballBehavior.CurrentScore;
		RestartCache.Bounces = _ballBehavior.CurrentBounces;
		RestartCache.StartBlockRotation = _ballLauncher.transform.rotation;
		RestartCache.Powerbar = _ballLauncher.LaunchPowerMeter;
		RestartCache.FlightPath = _ballBehavior.FlightPath;
		RestartCache.LoadFromCache = true;

		_ballBehavior.FiveBounceTrail.GetComponent<ParticleSystem>().Stop();
		_ballBehavior.FiveBounceTrail.GetComponent<ParticleSystem>().Clear();
		_ballBehavior.TenBounceTrail.GetComponent<ParticleSystem>().Stop();
		_ballBehavior.TenBounceTrail.GetComponent<ParticleSystem>().Clear();
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
			ReturnToLevelPicker(true);
	}

	public void NextWorldButton() {

		_selectedWorldID++;
		if (DataManager.SaveData.LevelList[-1].WorldID < _selectedWorldID) {
			_selectedWorldID--;
		} else {
			//Do world switching stuff.
		}
	}
	#endregion

	#region Misc
	void LoadLevelPicker() {

		Application.LoadLevel("LevelPicker");
	}

	public void ReturnToLevelPicker(bool immediate = false) {

		RestartCache.LoadFromCache = false;

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

		_ballLauncher.transform.rotation = RestartCache.StartBlockRotation;
		_ballBehavior.CurrentScore = RestartCache.Score;
		_ballBehavior.CurrentBounces = RestartCache.Bounces;
		_ballBehavior.UpdateScoreText();
		_ballBehavior.CurrentScore = 0;
		_ballBehavior.CurrentBounces = 0;
		_ballLauncher.LaunchMeterImage.fillAmount = RestartCache.Powerbar / _ballLauncher.MaxLaunchPower;
		_ballLauncher.SetPreviousPowerIndicator();
		_ballBehavior.FlightPath = RestartCache.FlightPath;
		_ballBehavior.DrawPreviousFlightPath();
	}

	void PopulateLevelListUI() {

		if (!_levelDisplayContent) {
			Debug.LogError("Content panel missing or renamed.");
		}
		Text levelName;
		Image padlock;
		Text scoreText;
		Text bounceText;
		Image crown;

		GameObject button;
		foreach (var level in DataManager.SaveData.LevelList) {
			if (level.WorldID < _selectedWorldID) { continue; }
			if (level.WorldID > _selectedWorldID) { break; }
			//TOOD: Have this not set every time.
			_worldName.text = World.Names[level.WorldID];
			button = Instantiate(LevelButtonResource);
			button.transform.SetParent(_levelDisplayContent.transform, false);

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
				if (level.BounceGoalUnlocked && level.LevelID != 1) {
					bounceText.text = string.Format("{0} / {1}", level.Bounces.ToString(), level.BounceGoal);
					crown.enabled = true;
				} else if (level.Score == 0) {
					bounceText.text = string.Format("{0} / ?", level.Bounces);
					crown.enabled = false;
				} else {
					bounceText.text = string.Format("{0} / {1}", level.Bounces.ToString(), level.BounceGoal);
					crown.enabled = false;
				}
			}
		}
	}

	public void DisplayWinDetails() {

		HighScoreStamp.transform.parent.GetComponent<HorizontalLayoutGroup>().enabled = true;
		//HighScoreStamp.transform.SetParent(LevelDisplayElementContainer);
		if (GotHighScore) {
			HighScoreStamp.GetComponent<Image>().enabled = true;
			HighScoreStamp.GetComponent<Animation>().Play();
			//HighScoreStamp.transform.FindChild("HighScoreParticles")
			//	.GetComponent<ParticleSystem>().Play();
			GotHighScore = false;
		}
		if (_ballBehavior.CurrentBounces >= CurrentLevel.BounceGoal && CurrentLevel.BounceGoal != 0) {
			//BounceGoalStamp.transform.SetParent(HighScoreStamp.transform.parent);
			BounceGoalStamp.SetActive(true);
			CurrentLevel.BounceGoalUnlocked = true;
			BounceGoalStamp.GetComponent<Image>().enabled = true;
			BounceGoalStamp.GetComponent<Animation>().Play();
		} else {
			BounceGoalStamp.SetActive(false);
			//BounceGoalStamp.transform.SetParent(HighScoreStamp.transform.parent.parent);
		}

	}

	public void MarkAsBusy() {

		IsBusy = true;
		Debug.Log("-------GameManager busy---------");
		if (Application.loadedLevel <= 2) {
			LoadingScreen.ActivateLoadingscreen(LoadingScreenManager.Screens.WallOfText);
		} else {
			LoadingScreen.ActivateLoadingscreen(LoadingScreenManager.Screens.Saving);
		}
	}

	public void MarkAsNotBusy() {

		IsBusy = false;
		Debug.Log("--------GameManager Not Busy----------");

		if (LoadingScreen.ActiveScreen != LoadingScreenManager.Screens.None) {
			LoadingScreen.DeactivateLoadingscreen();
		}
	}

	public void CheckAchievements(Collision colliderObject) {

		BallBehaviour ball = colliderObject.gameObject.GetComponent<BallBehaviour>();
		if (!DataManager.SaveData.AchievementProg.Pentabounce && ball.CurrentBounces >= 5) {
			//Social.ReportProgress(AchievementCodes.Pentabounce, 100f, (bool success) => {
			//	DataManager.SaveData.AchievementProg.Pentabounce = true;
			//});
			DataManager.SaveData.AchievementProg.Pentabounce = true;
			ShowAchievementPanel_tmp(AchievementCodes.CodeToNameDict[AchievementCodes.Pentabounce]);
		}
		if (!DataManager.SaveData.AchievementProg.TheDecabounce && ball.CurrentBounces >= 10) {
			//Social.ReportProgress(AchievementCodes.TheDecabounce, 100f, (bool success) => {
			//	DataManager.SaveData.AchievementProg.TheDecabounce = true;
			//});
			DataManager.SaveData.AchievementProg.TheDecabounce = true;
			ShowAchievementPanel_tmp(AchievementCodes.CodeToNameDict[AchievementCodes.TheDecabounce]);
			//DataManager.SaveData.UnlockedBalls.Add("BallBaseBall", true);
			//PlayerPrefs.SetInt("BallBaseBall", 1);
		}
		if (!DataManager.SaveData.AchievementProg.AScoreOfBounces && ball.CurrentBounces >= 20) {
			//Social.ReportProgress(AchievementCodes.AScoreOfBounces, 100f, (bool success) => {
			//	DataManager.SaveData.AchievementProg.AScoreOfBounces = true;
			//});
			DataManager.SaveData.AchievementProg.AScoreOfBounces = true;
			ShowAchievementPanel_tmp(AchievementCodes.CodeToNameDict[AchievementCodes.AScoreOfBounces]);
		}
		if (!DataManager.SaveData.AchievementProg.Check && GameManager.CurrentLevel.LevelID == 20) {
			//Social.ReportProgress(AchievementCodes.Check, 100f, (bool success) => {
			//	DataManager.SaveData.AchievementProg.Check = true;
			//});
			DataManager.SaveData.AchievementProg.Check = true;
			ShowAchievementPanel_tmp(AchievementCodes.CodeToNameDict[AchievementCodes.Check]);
			//DataManager.SaveData.UnlockedBalls.Add("BallEarth", true);
			//PlayerPrefs.SetInt("BallEarth", 1);
		}
		if (!DataManager.SaveData.AchievementProg.Champ && ball.CurrentScore >= 100) {
			//Social.ReportProgress(AchievementCodes.Champ, 100f, (bool success) => {
			//	DataManager.SaveData.AchievementProg.Champ = true;
			//});
			DataManager.SaveData.AchievementProg.Champ = true;
			ShowAchievementPanel_tmp(AchievementCodes.CodeToNameDict[AchievementCodes.Champ]);
			//DataManager.SaveData.UnlockedBalls.Add("BallPokeball", true);
			//PlayerPrefs.SetInt("Ball8Ball", 1);
		}
		if (!DataManager.SaveData.AchievementProg.Olympian && ball.CurrentScore >= 200) {
			//Social.ReportProgress(AchievementCodes.ADict[AchievementCodes.Olympian, 100f, (bool success) => {
			//	DataManager.SaveData.AchievementProg.Olympian = true;
			//});
			DataManager.SaveData.AchievementProg.Olympian = true;
			ShowAchievementPanel_tmp(AchievementCodes.CodeToNameDict[AchievementCodes.Olympian]);
		}
		if (!DataManager.SaveData.AchievementProg.YoureRidiculous && ball.CurrentScore >= 500) {
			//Social.ReportProgress(AchievementCodes.YoureRidiculous, 100f, (bool success) => {
			//	DataManager.SaveData.AchievementProg.YoureRidiculous = true;
			//});
			DataManager.SaveData.AchievementProg.YoureRidiculous = true;
			ShowAchievementPanel_tmp(AchievementCodes.CodeToNameDict[AchievementCodes.YoureRidiculous]);
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
				_audioWrangler.ChangeMusicVolume(.275f);
				_ballLauncher = GameObject.FindObjectOfType<BallLauncher>();
				// Now that Ball is static this reference isn't needed
				_ballBehavior = BallLauncher.Ball.GetComponent<BallBehaviour>();
				Text LevelText = _ballLauncher.transform.parent.FindChildRecursive("LevelText")
					.GetComponent<Text>();
				LevelText.text = string.Format("Level {0}", CurrentLevel.LevelID);
				if (RestartCache.LoadFromCache) {
					ApplyCacheToLoadedLevel();
				}
				break;
			case 0:
				LevelSelectBkGrndImage.enabled = false;
				_audioWrangler.ChangeMusicVolume(1f);
				break;
			case 1:
				LevelSelectBkGrndImage.enabled = true;
				_levelDisplayCanvasGO = GameObject.Find("LevelDisplayCanvas");
				_worldName = _levelDisplayCanvasGO.transform.FindChildRecursive("WorldName")
					.GetComponent<Text>();
				_levelDisplayContent = _levelDisplayCanvasGO.transform
					.FindChildRecursive("LevelDisplayContent").gameObject;
				PopulateLevelListUI();
				_audioWrangler.ChangeMusicVolume(1f);
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

	void OnGUI() {

		if (_loadError) {
			GUI.Box(new Rect(10, 10, 20, Screen.width), "Something went wrong while loading the data.");
		}
	}
}
