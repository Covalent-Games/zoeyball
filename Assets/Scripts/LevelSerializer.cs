using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace GameData {
	public class LevelSerializer {

		public List<DataManager.Level> DeserializeLevelData(TextAsset asset) {

			XmlSerializer serializer = new XmlSerializer(typeof(List<DataManager.Level>));
			using (var reader = new StringReader(asset.text)) {
				return (List<DataManager.Level>)serializer.Deserialize(reader);
			}
		}

		public byte[] SerializeLevelList(List<DataManager.Level> readableLevelList) {

			BinaryFormatter binaryFormatter = new BinaryFormatter();
			MemoryStream stream = new MemoryStream();
			binaryFormatter.Serialize(stream, readableLevelList);

			return stream.ToArray();
		}

		public List<DataManager.Level> DeserializeLevelList(byte[] data) {

			BinaryFormatter binaryFormatter = new BinaryFormatter();
			MemoryStream stream = new MemoryStream(data);
			object deserializedData = binaryFormatter.Deserialize(stream);

			return (List<DataManager.Level>)deserializedData;

		}
	}
}