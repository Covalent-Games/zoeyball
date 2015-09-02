using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LoadingScreenManager : MonoBehaviour {

	public enum Screens {
		None,
		WallOfText,
		Saving,
	}
	LoadingScreenManager.Screens ActiveScreen = Screens.None;
	Dictionary<Screens, Canvas> EnumToScreenDict;
	Canvas LoadingScreen_01;
	Canvas LoadingScreen_02;

	void Awake() {

		LoadingScreen_01 = transform.FindChild("Screen_01").GetComponent<Canvas>();
		LoadingScreen_02 = transform.FindChild("Screen_02").GetComponent<Canvas>();
	}

	void Start() {

		EnumToScreenDict = new Dictionary<Screens, Canvas>() {
			{Screens.WallOfText, LoadingScreen_01},
			{Screens.Saving, LoadingScreen_02},
		};
	}

	public void ActivateLoadingscreen(Screens num) {

		Debug.Log("Opening " + num.ToString());
		if (ActiveScreen == Screens.None || ActiveScreen == num) {
			EnumToScreenDict[num].enabled = true;
			ActiveScreen = num;
		} else {
			Debug.LogWarning("A loading screen tried to load while an existing was active!");
		}
	}

	public void DeactivateLoadingscreen() {

		Debug.Log("Closing " + ActiveScreen.ToString());
		EnumToScreenDict[ActiveScreen].enabled = false;
		ActiveScreen = Screens.None;
	}

	// Update is called once per frame
	void Update() {

	}
}
