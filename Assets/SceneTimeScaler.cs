using UnityEngine;
using System.Collections;

public class SceneTimeScaler : MonoBehaviour {

	public float timescale = 1f;

	// Use this for initialization
	void Update() {

		Time.timeScale = timescale;
	}


}
