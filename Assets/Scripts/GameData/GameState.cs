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
		public Dictionary<string, bool> UnlockedBalls;
		public string CurrentSelectedBallName = "BallYellow";
		#endregion

		public GameState() {

			LevelList = new List<Level>();
			LeaderBoardScores = new Dictionary<string, long>(){
				{ PlayServicesHandler.LeaderBoards.UpInTheClouds, 0L }
			};
			AchievementProg = new AchievementProgress();
			UnlockedBalls = new Dictionary<string, bool>();

			foreach (var ball in Designs.Balls) {
				UnlockedBalls.Add(ball, true);
				//if (ball == "BallYellow" || ball == "BallBlue") {
				//	UnlockedBalls.Add(ball, true);
				//	Debug.Log("-------------Unlocked " + ball);
				//} else if (PlayerPrefs.HasKey(ball) && PlayerPrefs.GetInt(ball) == 1) {
				//	UnlockedBalls.Add(ball, true);
				//	Debug.Log("-------------Unlocked " + ball);
				//}
			}

			if (PlayerPrefs.HasKey("CurrentBall")) {
				CurrentSelectedBallName = PlayerPrefs.GetString("CurrentBall");
			}
		}
	}
}
