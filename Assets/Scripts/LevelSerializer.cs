using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;


public class LevelSerializer {

	public void DeSerialize(out List<GameManager.Level> levelList, TextAsset asset) {

		XmlSerializer serializer = new XmlSerializer(typeof(List<GameManager.Level>));
		using (var reader = new StringReader(asset.text)) {
			levelList = (List<GameManager.Level>)serializer.Deserialize(reader);
		}
	}
}
