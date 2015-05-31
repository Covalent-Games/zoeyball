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
	public static GameManager Instance;
	public static Level CurrentLevel;
	public PlayServicesHandler GooglePlay;
	public TextAsset LevelData;
	public Canvas LevelCompleteCanvas;
	public Image LevelSelectBkGrndImage;
	public GameObject HighScoreStamp;
	public Animator MenuAnimator;
	public DataManager DataWrangler = new DataManager();
	public GameObject LevelButtonResource;
	public Text LevelText;
	public static bool GotHighScore = false;

	private AudioManager AudioWrangler;
	private BallBehaviour _BallBehavior;
	private BallLauncher _BallLauncher;

	private bool MenuOpen = false;
	#endregion

	#region Initializers
	void Awake() {

		GameManager.Instance = this;
		GooglePlay = new PlayServicesHandler();
		DataWrangler.LoadLevelTemplate(LevelData);
		AudioWrangler = GetComponent<AudioManager>();
		MenuAnimator = GameObject.FindGameObjectWithTag("MenuPanel").GetComponent<Animator>();
		MenuAnimator.enabled = false;
		LevelSelectBkGrndImage = GameObject.Find("LevelSelectBackground").GetComponent<Image>();
		DontDestroyOnLoad(gameObject);
	}

	void Start() {

		Time.timeScale = 1.15f;
		GooglePlay.Activate();
		GooglePlay.Authenticate();
	}
	#endregion

	#region Button Handlers
	public void Play() {

		try {
			DataWrangler.OnDataLoaded += LoadLevelPicker;
			DataWrangler.StartLoadGameData();
			DataWrangler.StartRecordingPlayTime();
		} catch (NullReferenceException) {
			Debug.Log("LoadGameData failed to execute");

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
		LoadLevelPicker();
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
		ToggleMenu();
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
		HighScoreStamp.GetComponent<RawImage>().enabled = false;

		if (toggleMenu) {
			ToggleMenu();
		}
		Application.LoadLevel(Application.loadedLevel);
	}

	public void NextButton() {

		RestartCache.LoadFromCache = false;

		LevelCompleteCanvas.enabled = false;
		HighScoreStamp.GetComponent<RawImage>().enabled = false;
		// If next level exists, load it.
		if (DataManager.SaveData.LevelList[CurrentLevel.LevelID] != null)
			LoadLevelByID(CurrentLevel.LevelID + 1);
		else
			ReturnToLevelPicker(true, true);
	}

	public void DisplaySaveSelection() {

		uint maxNumToDisplay = 10;
		bool allowCreateNew = false;
		bool allowDelete = true;

		ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
		savedGameClient.ShowSelectSavedGameUI("Select saved game",
			maxNumToDisplay,
			allowCreateNew,
			allowDelete,
			OnSavedGameSelected);
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

		GameObject contentPanel = GameObject.Find("Content");

		if (!contentPanel) {
			Debug.LogError("Content panel missing or renamed.");
		}

		GameObject button;
		foreach (var level in DataManager.SaveData.LevelList) {
			button = Instantiate(LevelButtonResource);
			button.transform.SetParent(contentPanel.transform, false);

			button.GetComponent<LoadLevelButton>().LevelToLoad = level;

			button.transform.FindChild("LevelName").GetComponent<Text>().text =
				string.Format("Level {0}", level.LevelID);
			button.transform.FindChild("ScoreText").GetComponent<Text>().text =
				Mathf.RoundToInt(level.Score).ToString();
			button.transform.FindChild("BounceText").GetComponent<Text>().text =
				level.Bounces.ToString();

			if (!level.IsUnlocked && level.LevelID != 1) {
				button.transform.FindChild("LevelName").GetComponent<Text>().color = Color.black;
			}
		}
	}

	public void DisplayWinDetails() {

		//GameObject ScoreRecapGO = LevelCompleteCanvas.transform.Find("LevelCompleteButtonLayout/Score Recap").gameObject;
		//ScoreRecapGO.GetComponent<Text>().text = string.Format("High Score: {0}\n" +
		//										 "New Score: {1}", Mathf.RoundToInt(CurrentLevel.Score), Mathf.RoundToInt(_BallBehavior.TmpScore));
		if (GotHighScore) {
			HighScoreStamp.GetComponent<RawImage>().enabled = true;
			HighScoreStamp.GetComponent<Animation>().Play();
			//HighScoreStamp.GetComponentInChildren<ParticleSystem>().Play();
			//GameObject HighScoreParticles = (GameObject)Instantiate(HighScoreStamp.transform.GetChild(0).gameObject, HighScoreStamp.transform.position, HighScoreStamp.transform.rotation);
			//HighScoreParticles.GetComponent<ParticleSystem>().Play();
			GotHighScore = false;
		}

	}
	#endregion

	#region Event Subs
	void OnApplicationQuit() {

		PlayerPrefs.Save();
	}

	void OnLevelWasLoaded(int levelIndex) {


		switch (levelIndex) {
			default:
				LevelSelectBkGrndImage.enabled = false;
				AudioWrangler.ChangeMusicVolume(.275f);
				_BallLauncher = GameObject.FindObjectOfType<BallLauncher>();
				_BallBehavior = _BallLauncher.Ball.GetComponent<BallBehaviour>();
				Text LevelText = _BallLauncher.transform.parent.FindChildRecursive("LevelText").GetComponent<Text>();
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
				PopulateLevelListUI();
				AudioWrangler.ChangeMusicVolume(1f);
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
