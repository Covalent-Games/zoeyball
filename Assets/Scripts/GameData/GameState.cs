using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GameData {

	[System.Serializable]
	public class GameState {

		public List<Level> LevelList;
		public Dictionary<string, long> LeaderBoardScores;
		#region Achievement bools
		public AchievementProgress AchievementProg;
		#endregion

		public GameState() {

			LevelList = new List<Level>();
			LeaderBoardScores = new Dictionary<string, long>(){
				{ PlayServicesHandler.LeaderBoards.UpInTheClouds, 0L }
			};
			AchievementProg = new AchievementProgress();
		}
	}
}
