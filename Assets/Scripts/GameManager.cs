using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using System;

[RequireComponent(typeof(PlayServicesHandler))]
public class GameManager : MonoBehaviour {

	public static DataManager.Level CurrentLevel;
	public TextAsset LevelData;
	public Canvas EscapeMenuCanvas;
	public Canvas RestartButtonCanvas;
	public Canvas LevelCompleteCanvas;
	AudioManager AudioWrangler;
	public DataManager DataWrangler = new DataManager();
	public GameObject LevelButtonResource;

	private BallBehaviour _BallBehavior;
	private BallLauncher _BallLauncher;


	void Awake() {

		DataWrangler.LoadLevelTemplate(LevelData);
		AudioWrangler = GetComponent<AudioManager>();
		DontDestroyOnLoad(gameObject);
		DontDestroyOnLoad(EscapeMenuCanvas.gameObject);
	}

	void Start() {

		Time.timeScale = 1.15f;
		PlayServicesHandler.Activate();
		PlayServicesHandler.Authenticate();
	}

	void Update() {

		if (Input.GetKeyDown(KeyCode.Escape)) {
			EscapeMenuCanvas.enabled = !EscapeMenuCanvas.enabled;
		}
	}

	public void Play() {

		try {
			DataWrangler.OnDataLoaded += LoadLevelPicker;
			DataWrangler.StartLoadGameData();
		} catch (NullReferenceException) {
			Debug.Log("LoadGameData failed to execute");

			if (Application.isEditor) {
				Debug.LogWarning(
					"Loading LevelPicker. You're running in the Editor so no save files used!");
				LoadLevelPicker();
			}
		}
		LoadLevelPicker();
		}

	void LoadLevelPicker() {

		Application.LoadLevel("LevelPicker");
	}

	public void DisplaySaveSelection() {

		uint maxNumToDisplay = 5;
		bool allowCreateNew = false;
		bool allowDelete = true;

		ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
		savedGameClient.ShowSelectSavedGameUI("Select saved game",
			maxNumToDisplay,
			allowCreateNew,
			allowDelete,
			OnSavedGameSelected);
	}

	void OnSavedGameSelected(SelectUIStatus status, ISavedGameMetadata metaData) {

		if (status == SelectUIStatus.SavedGameSelected) {
			Debug.Log("Selected the saved game");
		} else {
			Debug.LogError("The save game selection got borked");
		}
	}

	public void Quit() {

		DataWrangler.StartSaveGameData();
		Application.Quit();
	}

	public void ReturnButton() {

		EscapeMenuCanvas.enabled = false;
		LevelCompleteCanvas.enabled = false;
		ReturnToLevelPicker(true, true);
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

	public void Restart() {

		RestartCache.Score = _BallBehavior.TmpScore;
		RestartCache.Bounces = _BallBehavior.TmpBounces;
		RestartCache.StartBlockRotation = _BallLauncher.transform.rotation;
		RestartCache.Powerbar = _BallLauncher.LaunchPowerMeter;
		RestartCache.LoadFromCache = true;

		EscapeMenuCanvas.enabled = false;
		LevelCompleteCanvas.enabled = false;
		Application.LoadLevel(Application.loadedLevel);
	}

	public void NextButton() {

		RestartCache.LoadFromCache = false;

		LevelCompleteCanvas.enabled = false;
		// If next level exists, load it.
		if (DataManager.LevelList[CurrentLevel.LevelID] != null)
			LoadLevelByID(CurrentLevel.LevelID + 1);
		else
			ReturnToLevelPicker(true, true);
	}

	public static void LoadLevelByID(int ID) {

		if (ID != 1 && !DataManager.LevelList[ID - 1].IsUnlocked) { return; }

		Application.LoadLevel(string.Format("Level{0}", ID));
		GameManager.CurrentLevel = DataManager.LevelList[ID - 1];

	}

	void OnLevelWasLoaded(int levelIndex) {

		switch (levelIndex) {
			default:
				RestartButtonCanvas.enabled = true;
				AudioWrangler.ChangeMusicVolume(.275f);
				_BallLauncher = GameObject.FindObjectOfType<BallLauncher>();
				_BallBehavior = _BallLauncher.Ball.GetComponent<BallBehaviour>();
				if (RestartCache.LoadFromCache) {
					ApplyCacheToLoadedLevel();
				}
				break;
			case 0:
				RestartButtonCanvas.enabled = false;
				AudioWrangler.ChangeMusicVolume(1f);
				break;
			case 1:
				RestartButtonCanvas.enabled = false;
				PopulateLevelListUI();
				AudioWrangler.ChangeMusicVolume(1f);
				break;
		}
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

		foreach (var level in DataManager.LevelList) {
			GameObject button = Instantiate(LevelButtonResource);

			button.transform.SetParent(contentPanel.transform, false);
			LoadLevelButton loadLevelButton = button.GetComponent<LoadLevelButton>();
			loadLevelButton.LevelToLoad = level;
			Text nameText = button.transform.FindChild("LevelName").GetComponent<Text>();
			nameText.text = level.Name;
			button.transform.FindChild("ScoreText").GetComponent<Text>().text =
					Mathf.RoundToInt(level.Score).ToString();
			button.transform.FindChild("BounceText").GetComponent<Text>().text = level.Bounces.ToString();

			if (!level.IsUnlocked && level.LevelID != 1) {
				nameText.color = Color.black;
			}
		}
	}

	public void DisplayWinDetails() {

		GameObject ScoreRecapGO = LevelCompleteCanvas.transform.FindChild("Score Recap").gameObject;
		ScoreRecapGO.GetComponent<Text>().text = string.Format("Old Score: \n" +
												 "New Score: {0}\n" +
												 "Some other stuff...", Mathf.RoundToInt(_BallBehavior.TmpScore));
	}


}
