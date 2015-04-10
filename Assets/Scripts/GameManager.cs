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
	AudioManager AudioWrangler;


	GameObject LevelButtonResource;

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
		ReturnToLevelPicker(true, true);
	}

	public void ReturnToLevelPicker(bool save, bool immediate = false) {


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

		EscapeMenuCanvas.enabled = false;
		Application.LoadLevel(Application.loadedLevel);
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


}
