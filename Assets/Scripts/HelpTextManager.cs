using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class HelpTextManager : MonoBehaviour {

	public Canvas HelpTextCanvas;
	public Text HelpText;
	int _CurrentTextIndex = -1;
	public List<string> HelpTextList = new List<string>();

	void Start() {

		bool showHelpText = true;
		string level = gameObject.name.Split('_')[1];
		switch (level) {
			case "1":
				if (PlayerPrefs.HasKey("Level1HelpText")) {
					showHelpText = false;
				}
				break;
			case "2":
				if (PlayerPrefs.HasKey("Level2HelpText")) {
					showHelpText = false;
				}
				break;
			case "3":
				if (PlayerPrefs.HasKey("Level3HelpText")) {
					showHelpText = false;
				}
				break;
			case "11":
				if (PlayerPrefs.HasKey("Level11HelpText")) {
					showHelpText = false;
				}
				break;
		}

		if (showHelpText)
			DisplayNextText();
		else
			HelpTextCanvas.enabled = false;
	}

	public void DisplayNextText() {

		_CurrentTextIndex++;

		if (_CurrentTextIndex >= HelpTextList.Count) {
			HelpTextCanvas.enabled = false;
			return;
		}

		HelpText.text = HelpTextList[_CurrentTextIndex];
	}

	public void SeeLevel1HelpText() {

		PlayerPrefs.SetString("Level1HelpText", "Yes");
	}

	public void SeeLevel2HelpText() {

		PlayerPrefs.SetString("Level2HelpText", "Yes");
	}

	public void SeeLevel3HelpText() {

		PlayerPrefs.SetString("Level3HelpText", "Yes");
	}

	public void SeeLevel11HelpText() {

		PlayerPrefs.SetString("Level11HelpText", "Yes");
	}
}

