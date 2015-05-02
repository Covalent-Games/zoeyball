using UnityEngine;
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

		ISavedGameMetadata SavedGameMetaData;
		public LevelSerializer Serializer = new LevelSerializer();
		SessionTimeTracker thisTimeTracker = new SessionTimeTracker();
		public static List<Level> LevelList = new List<Level>();

		[System.Serializable]
		public class Level {
			public string Name;
			public int LevelID;
			public int WorldID;
			[XmlIgnore]
			public bool IsUnlocked = false;
			[XmlIgnore]
			public float Score;
			[XmlIgnore]
			public int Bounces;
		}
		public delegate void DataChangedEventHandler();
		public event DataChangedEventHandler OnDataLoaded;
		public event DataChangedEventHandler OnDataSaved;
		public event DataChangedEventHandler OnDataDeleted;

		bool Writing = false;

		/// <summary>
		/// Loads the basic level template. This is not the player's progress.
		/// </summary>
		/// <param name="levelData">The TextAsset XML file containing level info.</param>
		public void LoadLevelTemplate(TextAsset levelData) {

			LevelList = Serializer.Deserialize(levelData);
		}

		public void StartRecordingPlayTime() {

			thisTimeTracker.StartRecordingTime();
		}

		#region Game Saving/Loading
		public void OpenSavedGame() {

			ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;

			if (Application.isEditor) {
				Debug.LogWarning("Skipping Google Play Services. Loading LevelPicker.");
				Application.LoadLevel("LevelPicker");
			}

			savedGameClient.OpenWithAutomaticConflictResolution(
					"save",
					DataSource.ReadCacheOrNetwork,
					ConflictResolutionStrategy.UseLongestPlaytime,
					OnSavedGameOpened);
		}

		public void StartSaveGameData() {

			OpenSavedGame();
			Writing = true;
		}

		public void StartLoadGameData() {

			OpenSavedGame();
			Writing = false;
		}
		#endregion
		#region Data Manager Callbacks

		void OnSavedGameOpened(SavedGameRequestStatus status, ISavedGameMetadata metaData) {

			switch (status) {
				case SavedGameRequestStatus.Success:
					SavedGameMetaData = metaData;

					// Set to save the current game.
					if (Writing) {
						byte[] savedData = Serializer.SerializeLevelList(LevelList);

						ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
						SavedGameMetadataUpdate.Builder builder = new SavedGameMetadataUpdate.Builder();

						builder = builder
							.WithUpdatedDescription("Saved game at " + DateTime.Now)
							.WithUpdatedPlayedTime(
								thisTimeTracker.GetTimeSnapshot(metaData.TotalTimePlayed));

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
					OnDataSaved();
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
		}

		void OnSavedGameDataRead(SavedGameRequestStatus status, byte[] data) {

			switch (status) {
				case SavedGameRequestStatus.Success:
					LevelList = Serializer.DeserializeLevelList(data);

					// Events to trigger once data has been loaded
					if (OnDataLoaded != null) {
						OnDataLoaded();
					} else {
						Debug.LogError("No event registered for OnDataLoaded. LevelPicker scene won't be loaded");
					}

					break;
				case SavedGameRequestStatus.AuthenticationError:
					Debug.Log("LOAD FAILED! NO AUTHENTICATION!");
					break;
				case SavedGameRequestStatus.BadInputError:
					Debug.Log("LOAD FAILED! BAD INPUT!");
					break;
				case SavedGameRequestStatus.InternalError:
					Debug.Log("LOAD FAILED BUT IT LIKELY ISN'T OUR FAULT! INTERNAL ERROR!");
					break;
				case SavedGameRequestStatus.TimeoutError:
					Debug.Log("TIMEOUT LOADING FILE");
					break;
			}
		}

		#endregion

	}
}