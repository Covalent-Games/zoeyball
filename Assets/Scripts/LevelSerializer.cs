using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;


public class LevelSerializer {

	public void DeSerialize(ref List<GameManager.Level> levelList) {

		XmlSerializer serializer = new XmlSerializer(typeof(List<GameManager.Level>));
		using (TextReader reader = new StreamReader(Application.dataPath + "/Levels/leveldata.xml")) {
			levelList = (List<GameManager.Level>)serializer.Deserialize(reader);
		}
	}
}
