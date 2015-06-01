using UnityEngine;
using System.Collections;

namespace GameData {
	[System.Serializable]
	public class AchievementProgress {
		// Mark true when completed
		public bool Check = false;
		public bool Pentabounce = false;
		public bool TheDecabounce = false;
		public bool AScoreOfBounces = false;
		public bool IThinkYouMissed = false;
	} 
}
