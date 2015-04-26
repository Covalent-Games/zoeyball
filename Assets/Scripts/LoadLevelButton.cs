using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadLevelButton : MonoBehaviour {

	public DataManager.Level LevelToLoad;

	public void LoadLevel() {

		GameManager.LoadLevelByID(LevelToLoad.LevelID);
	}
}
