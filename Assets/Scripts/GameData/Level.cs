using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System;

namespace GameData {

	[System.Serializable]
	public class Level {

		public int LevelID;
		public int WorldID;
		public int BounceGoal;
		[XmlIgnore]
		public bool BounceGoalUnlocked = false;
		[XmlIgnore]
		public bool IsUnlocked = false;
		[XmlIgnore]
		public float Score;
		[XmlIgnore]
		public int Bounces;
	}
}
