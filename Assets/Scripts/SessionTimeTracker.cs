using System;
using System.Collections;
using UnityEngine;

public class SessionTimeTracker {

	public const int NanoSecondDivider = 10000;
	DateTime RecordingStartTime;
	int PauseDuration;
	bool TrackingStarted = false;

	//TODO: Overload to specify start time
	/// <summary>
	/// Captures time snapshot to be used to record a duration of time. 
	/// </summary>
	/// <returns></returns>
	public DateTime StartRecordingTime() {

		RecordingStartTime = DateTime.Now;
		TrackingStarted = true;

		return RecordingStartTime;
	}

	public TimeSpan GetTimeSnapshot(TimeSpan currentTime) {

		if (!TrackingStarted) {
			Debug.LogError("Timespan was requested but the timer was never started!");
		}

		TimeSpan timeSpan;

		long duration = DateTime.Now.Ticks - RecordingStartTime.Ticks;
		int milliseconds = (int)duration / SessionTimeTracker.NanoSecondDivider;
		timeSpan = new TimeSpan(0, 0, 0, 0, milliseconds);

		// Restart Timer.
		RecordingStartTime = DateTime.Now;

		return currentTime + timeSpan;
	}
}
