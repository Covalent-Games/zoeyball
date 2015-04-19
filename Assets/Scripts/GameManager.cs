using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

public class GameManager : MonoBehaviour {

	public static Level CurrentLevel;
	public TextAsset LevelData;
	public Canvas EscapeMenuCanvas;
	public Canvas RestartButtonCanvas;
	public Canvas LevelCompleteCanvas;
	AudioManager AudioWrangler;

	GameObject LevelButtonResource;
	private BallBehaviour _BallBehavior;
	private BallLauncher _BallLauncher;


	[System.Serializable]
	public class Level {
		public string Name;
		public int LevelID;
		public int WorldID;
		[XmlIgnore]
		public bool IsUnlocked = false;
		[XmlIgnore]
		public float Score;
		[XmlIgnore]
		public int Bounces;
	}

	public List<Level> LevelList = new List<Level>();
	LevelSerializer Serializer;

	void Awake() {

		Serializer = new LevelSerializer();
		Serializer.DeSerialize(out LevelList, LevelData);
		AudioWrangler = GetComponent<AudioManager>();
		DontDestroyOnLoad(gameObject);
		DontDestroyOnLoad(EscapeMenuCanvas.gameObject);

		UIResources.Load();

	}

	void Start() {

		LevelButtonResource = UIResources.UIObjects["LevelButton"];
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

		if (!DataManager.Load(ref LevelList)) {
			DataManager.Save(LevelList);
			Debug.Log("A new save file was created.");
		}
		Application.LoadLevel("LevelPicker");
	}

	public void Quit() {

		DataManager.Save(LevelList);
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
			DataManager.Save(LevelList);
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
		if (LevelList[CurrentLevel.LevelID] != null)
			LoadLevelByID(CurrentLevel.LevelID + 1);
		else
			ReturnToLevelPicker(true, true);
	}

	public static void LoadLevelByID(int ID) {

		GameManager gm = GameObject.FindObjectOfType<GameManager>();

		if (ID != 1 && !gm.LevelList[ID - 1].IsUnlocked) { return; }

		Application.LoadLevel(string.Format("Level{0}", ID));
		GameManager.CurrentLevel = gm.LevelList[ID - 1];

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

		foreach (Level level in LevelList) {
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
