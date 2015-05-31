using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using GameData;

public class LoadLevelButton : MonoBehaviour {

	public Level LevelToLoad;

	public void LoadLevel() {

		GameManager.LoadLevelByID(LevelToLoad.LevelID);
	}
}
