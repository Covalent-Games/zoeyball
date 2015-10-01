using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class RestartCache {

	public static Quaternion StartBlockRotation;
	public static float Powerbar;
	public static float Score;
	public static int Bounces;
	public static List<Vector3> FlightPath;

	public static bool LoadFromCache = false;
}
