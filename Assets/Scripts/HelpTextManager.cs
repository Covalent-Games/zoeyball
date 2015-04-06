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

		DisplayNextText();
	}

	public void DisplayNextText() {

		_CurrentTextIndex++;

		if (_CurrentTextIndex >= HelpTextList.Count) {
			HelpTextCanvas.enabled = false;
			return;
		}

		HelpText.text = HelpTextList[_CurrentTextIndex];
	}
}

