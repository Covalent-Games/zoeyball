using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace GameData {
	public class GameStateSerializer {

		public List<Level> DeserializeLevelList(TextAsset asset) {

			XmlSerializer serializer = new XmlSerializer(typeof(List<Level>));
			using (var reader = new StringReader(asset.text)) {
				return (List<Level>)serializer.Deserialize(reader);
			}
		}

		public byte[] SerializeGameState(GameState gameState) {

			BinaryFormatter binaryFormatter = new BinaryFormatter();
			MemoryStream stream = new MemoryStream();
			binaryFormatter.Serialize(stream, gameState);

			return stream.ToArray();
		}

		public GameState DeserializeGameState(byte[] data) {

			BinaryFormatter binaryFormatter = new BinaryFormatter();
			MemoryStream stream = new MemoryStream(data);
			object deserializedData = binaryFormatter.Deserialize(stream);

			return (GameState)deserializedData;

		}
	}
}