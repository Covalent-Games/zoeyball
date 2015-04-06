using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class DataManager : MonoBehaviour {

	public static bool Save(List<GameManager.Level> saveObject) {

		using (Stream stream = File.Create(Application.persistentDataPath + "/savedata.dat")) {
			var bFormatter = new BinaryFormatter();
			bFormatter.Serialize(stream, saveObject);
		}
		Debug.Log("Saved");
		return true;
	}

	public static bool Load(ref List<GameManager.Level> loadObject) {

		if (!File.Exists(Application.persistentDataPath + "/savedata.dat")) { return false; }

		using (Stream stream = File.Open(Application.persistentDataPath + "/savedata.dat", FileMode.Open)) {
			var bFormatter = new BinaryFormatter();
			List<GameManager.Level> tmpList = bFormatter.Deserialize(stream) as List<GameManager.Level>;

			for (int index = tmpList.Count; index < loadObject.Count; index++) {
				Debug.Log(loadObject[index].Name);
			}

			loadObject = tmpList;
		}

		Debug.Log("Loaded");
		return true;
	}
}
