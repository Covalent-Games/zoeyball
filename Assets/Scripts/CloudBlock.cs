using UnityEngine;
using System.Collections;

public class CloudBlock : MonoBehaviour {

	void OnCollisionEnter(Collision collision) {

		if (collision.gameObject.CompareTag("Ball")) {
			StartCoroutine(PopCloudRoutine());
		}
	}

	private IEnumerator PopCloudRoutine() {

		// Wait one frame for bounce physics to complete
		yield return null;

		// TODO: play *poof sound

		gameObject.SetActive(false);
	}
}
