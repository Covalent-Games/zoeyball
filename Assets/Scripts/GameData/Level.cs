using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System;

namespace GameData {

	[System.Serializable]
	public class Level {

		public int LevelID;
		public int WorldID;
		[XmlIgnore]
		public bool IsUnlocked = false;
		[XmlIgnore]
		public float Score;
		[XmlIgnore]
		public int Bounces;
	}
}
