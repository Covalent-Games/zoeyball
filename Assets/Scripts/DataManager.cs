using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class DataManager : MonoBehaviour {

	public static bool Save(List<GameManager.Level> saveObject) {

		foreach (var level in saveObject) {
			Debug.Log(level.LevelID + " -- " + level.Score);
		}

		using (Stream stream = File.Create(Application.persistentDataPath + @"/savedata.dat")) {
			var bFormatter = new BinaryFormatter();
			bFormatter.Serialize(stream, saveObject);
			stream.Close();
		}
		Debug.Log("Saved");
		return true;
	}

	public static bool Load(ref List<GameManager.Level> loadObject) {

		foreach (var level in loadObject) {
			Debug.Log(level.LevelID + " -- " + level.Score);
		}

		if (!File.Exists(Application.persistentDataPath + @"/savedata.dat")) { return false; }

		using (Stream stream = File.Open(Application.persistentDataPath + @"/savedata.dat", FileMode.Open)) {
			var bFormatter = new BinaryFormatter();
			loadObject = bFormatter.Deserialize(stream) as List<GameManager.Level>;
			stream.Close();
		}

		Debug.Log("Loaded");
		return true;
	}
}
