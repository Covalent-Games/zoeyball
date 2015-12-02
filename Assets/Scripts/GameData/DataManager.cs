﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using GooglePlayGames;
using GooglePlayGames.BasicApi.SavedGame;
using System;

/// <summary>
/// Handles all data.
/// </summary>

namespace GameData {

	public class DataManager {

		#region Members
		public const string SaveFileName = "save_alpha_test_02";
		public static GameState SaveData = new GameState();
		public static GameObject[] BallObjects;
		public static Dictionary<string, string> BallNamePathPairs = new Dictionary<string, string>();
		public GameStateSerializer Serializer = new GameStateSerializer();

		public delegate void DataChangedEventHandler();
		public event DataChangedEventHandler OnDataChangeStarted;
		public event DataChangedEventHandler OnDataLoaded;
		public event DataChangedEventHandler OnDataSaved;
		public event DataChangedEventHandler OnDataSaveFailedAny;
		public event DataChangedEventHandler OnFailedAuthentication;
		public event DataChangedEventHandler OnInternalError;
		public event DataChangedEventHandler OnTimeOut;
		public event DataChangedEventHandler OnBadInput;
		//public event DataChangedEventHandler OnDataDeleted;

		private SessionTimeTracker _timeTracker = new SessionTimeTracker();
		ISavedGameMetadata SavedGameMetaData;
		bool Writing = false;
		// Currently means literally the opposite.
		bool ResolvingConflict = true;
		#endregion

		public DataManager() {

			if (Application.isEditor) {
				DirectoryInfo di = new DirectoryInfo(Application.dataPath + "/Resources/Balls");
				FileInfo[] files = di.GetFiles("*.prefab");
				foreach (FileInfo file in files) {
					string name = Path.GetFileNameWithoutExtension(file.Name);
					BallNamePathPairs.Add(name, "Balls/" + name);
					// Check if it's not a default unlocked ball
					if (name != "BallBlue" && name != "BallYellow") {
						if (PlayerPrefs.HasKey(name) && !SaveData.UnlockedBalls.ContainsKey(name)) {
							SaveData.UnlockedBalls.Add(name, true);
						}
					}
				}
			} else {
				foreach (var ball in Designs.Balls) {
					BallNamePathPairs.Add(ball, "Balls/" + ball);
				}
			}
		}

		/// <summary>
		/// Loads the basic level template. This is not the player's progress.
		/// </summary>
		/// <param name="levelData">The TextAsset XML file containing level info.</param>
		public void LoadLevelTemplate(TextAsset levelData) {

			SaveData.LevelList = Serializer.DeserializeLevelList(levelData);
		}

		public void StartRecordingPlayTime() {

			_timeTracker.StartRecordingTime();
		}

		public void StartSaveGameData() {
			if (OnDataChangeStarted != null) {
				OnDataChangeStarted();
			}
			Writing = true;
			OpenSavedGame();
		}

		public void StartLoadGameData() {

			if (OnDataChangeStarted != null) {
				OnDataChangeStarted();
			}
			Writing = false;
			OpenSavedGame();
		}

		#region Game Saving/Loading
		public void OpenSavedGame() {

			ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;

			if (Application.isEditor) {
				Debug.LogWarning("Skipping Google Play Services. Loading LevelPicker.");

				GameManager.Instance.MarkAsNotBusy();

				Application.LoadLevel("LevelPicker");
				return;
			}

			// The following code doesn't do what it looks like it does. So... ya. It may or may not in the future.

			// If there was a save game conflict then we've just written a new, merged save file to the cloud
			// Now we're opening it with the new metadata TimeSpan, and it SHOULD have the longest playtime.
			if (ResolvingConflict) {
				//ResolvingConflict = false;
				savedGameClient.OpenWithAutomaticConflictResolution(
					SaveFileName,
					DataSource.ReadCacheOrNetwork,
					ConflictResolutionStrategy.UseLongestPlaytime,
					OnSavedGameOpened);
				// This is the standard way of opening a file, and if we detect a conflict we'll merge files and
				// come back to this again.
			} else {
				bool prefetchDataOnConflict = true;
				savedGameClient.OpenWithManualConflictResolution(
						SaveFileName,
						DataSource.ReadCacheOrNetwork,
						prefetchDataOnConflict,
						OnSavedGameConflict,
						OnSavedGameOpened);
			}
		}
		#endregion

		#region Data Manager Callbacks

		void OnSavedGameConflict(IConflictResolver resolver, ISavedGameMetadata original, byte[] originalData,
			ISavedGameMetadata unmerged, byte[] unmergedData) {

			ResolvingConflict = true;

			GameState originalState = Serializer.DeserializeGameState(originalData);
			GameState unmergedState = Serializer.DeserializeGameState(unmergedData);

			for (int i = 0; i < unmergedState.LevelList.Count; i++) {
				if (originalState.LevelList[i].Score < unmergedState.LevelList[i].Score) {
					originalState.LevelList[i] = unmergedState.LevelList[i];
				}
			}

			_timeTracker.FalseTimeForConflicts = original.TotalTimePlayed + unmerged.TotalTimePlayed;
			DataManager.SaveData = originalState;
			StartSaveGameData();

		}

		void OnSavedGameOpened(SavedGameRequestStatus status, ISavedGameMetadata metaData) {
			switch (status) {
				case SavedGameRequestStatus.Success:
					SavedGameMetaData = metaData;

					// Set to save the current game.
					if (Writing) {
						byte[] savedData = Serializer.SerializeGameState(DataManager.SaveData);

						ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
						SavedGameMetadataUpdate.Builder builder = new SavedGameMetadataUpdate.Builder();

						// If there was a save file conflict then we want the "fake" merged time.
						TimeSpan playTime = metaData.TotalTimePlayed;

						builder = builder
							.WithUpdatedDescription("Saved game at " + DateTime.Now)
							.WithUpdatedPlayedTime(
								_timeTracker.GetTimeSnapshot(playTime));

						SavedGameMetadataUpdate updatedMetadata = builder.Build();
						savedGameClient.CommitUpdate(
								SavedGameMetaData,
								updatedMetadata,
								savedData,
								OnSavedGameWritten);

						// Set to load an existing game.
					} else {
						ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
						savedGameClient.ReadBinaryData(SavedGameMetaData, OnSavedGameDataRead);
					}

					break;
				case SavedGameRequestStatus.AuthenticationError:
					Debug.Log("OPEN FAILED! NO AUTHENTICATION!");
					break;
				case SavedGameRequestStatus.BadInputError:
					Debug.Log("OPEN FAILED! BAD INPUT!");
					break;
				case SavedGameRequestStatus.InternalError:
					Debug.Log("OPEN FAILED BUT IT LIKELY ISN'T OUR FAULT! INTERNAL ERROR!");
					break;
				case SavedGameRequestStatus.TimeoutError:
					Debug.Log("TIMEOUT OPENING FILE");
					break;
			}

		}

		void OnSavedGameWritten(SavedGameRequestStatus status, ISavedGameMetadata metaData) {

			switch (status) {
				case SavedGameRequestStatus.Success:
					if (OnDataSaved != null) {
						Debug.Log("OnDataSaved event");
						OnDataSaved();
					}
					break;
				case SavedGameRequestStatus.AuthenticationError:
					Debug.Log("SAVE FAILED! NO AUTHENTICATION!");
					break;
				case SavedGameRequestStatus.BadInputError:
					Debug.Log("SAVE FAILED! BAD INPUT!");
					break;
				case SavedGameRequestStatus.InternalError:
					Debug.Log("SAVE FAILED BUT IT LIKELY ISN'T OUR FAULT! INTERNAL ERROR!");
					break;
				case SavedGameRequestStatus.TimeoutError:
					Debug.Log("TIMEOUT SAVING FILE");
					break;
			}
			if (status != SavedGameRequestStatus.Success) {
				if (OnDataSaveFailedAny != null) {
					OnDataSaveFailedAny.Invoke();
				}
			}
		}

		void OnSavedGameDataRead(SavedGameRequestStatus status, byte[] incomingData) {

			switch (status) {
				case SavedGameRequestStatus.Success:
					if (incomingData.Length == 0) {
						// If stuff starts happening and shouldn't after loading, this might be why!!!
						OnDataLoaded();
						return;
					}
					GameState incomingGameState = Serializer.DeserializeGameState(incomingData);

					// This is the common result. The App has new levels that we don't want to delete with
					// the incoming saved game, so we update the level list up to the last saved level.
					if (incomingGameState.LevelList.Count <= DataManager.SaveData.LevelList.Count) {
						for (int i = 0; i < incomingGameState.LevelList.Count; i++) {
							DataManager.SaveData.LevelList[i] = incomingGameState.LevelList[i];
						}
					} else {
						// The SavedGame is more up to date than the app. This should be rare. Here we're
						// safe to completely overwrite the local data. In this case the incoming data will
						// have more levels than the local XML. 
						//TODO: Consider prompting the user to update their app if this happens.
						DataManager.SaveData.LevelList = incomingGameState.LevelList;
					}
					DataManager.SaveData.AchievementProg = incomingGameState.AchievementProg;
					DataManager.SaveData.LeaderBoardScores = incomingGameState.LeaderBoardScores;
					OnDataLoaded();
					// Loop through all the unlocked/locked balls to guarantee we don't overwrite anything
					// that's already unlocked.
					//foreach (var entry in tmpGameState.UnlockedBalls) {
					//	// If the cache says the ball isn't unlocked...
					//	if (!SaveData.UnlockedBalls[entry.Key]) {
					//		// Check if the cloud save says it is...
					//		if (tmpGameState.UnlockedBalls[entry.Key]) {
					//			// then overwite the cache with the unlocked ball.
					//			SaveData.UnlockedBalls[entry.Key] = tmpGameState.UnlockedBalls[entry.Key];
					//		}
					//	}
					//}

					// Events to trigger once data has been loaded
					if (OnDataLoaded != null) {
						Debug.Log("----------OnDataLoaded event------------");
						OnDataLoaded();
					} else {
						Debug.LogError("No event registered for OnDataLoaded. LevelPicker scene won't be loaded.");
					}
					break;
				case SavedGameRequestStatus.AuthenticationError:
					Debug.Log("LOAD FAILED! NO AUTHENTICATION!");
					if (OnFailedAuthentication != null)
						OnFailedAuthentication();
					break;
				case SavedGameRequestStatus.BadInputError:
					Debug.Log("LOAD FAILED! BAD INPUT!");
					if (OnBadInput != null)
						OnBadInput();
					break;
				case SavedGameRequestStatus.InternalError:
					Debug.Log("LOAD FAILED BUT IT LIKELY ISN'T OUR FAULT! INTERNAL ERROR!");
					if (OnInternalError != null)
						OnInternalError();
					break;
				case SavedGameRequestStatus.TimeoutError:
					Debug.Log("TIMEOUT LOADING FILE");
					if (OnTimeOut != null)
						OnTimeOut();
					break;
			}
		}
		#endregion
	}
}