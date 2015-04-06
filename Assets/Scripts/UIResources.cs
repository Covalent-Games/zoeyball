using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class UIResources {

	public static Dictionary<string, GameObject> UIObjects = new Dictionary<string, GameObject>();
	public static Dictionary<string, GameObject> UISprites = new Dictionary<string, GameObject>();

	public static void Load() {

		foreach (string path in new List<string>() {
				"LevelButton" 
			}) {

			UIObjects.Add(path, Resources.Load(path) as GameObject);
		}

		foreach (string objName in new List<string>() {
				"MuteMusicIcon_Muted",
				"MuteMusicIcon_NotMuted",
				"MuteSoundsIcon_Muted",
				"MuteSoundsIcon_NotMuted",
			}) {

			UISprites.Add(objName, Resources.Load("Images/" + objName) as GameObject);
		}
	}
}
