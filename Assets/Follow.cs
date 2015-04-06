using UnityEngine;
using System.Collections;

public class Follow : MonoBehaviour {

	public GameObject FollowTarget;
	[Range(0.01f, 1.0f)]
	public float FollowDistance;
	GameObject FocusObject;
	public GameObject WinBlock;

	void Start() {

		if (!FollowTarget) {
			FollowTarget = GameObject.Find("Ball");
			if (!FollowTarget) {
				this.enabled = false;
			}
		}

		if (!WinBlock) {
			WinBlock = GameObject.Find("WinBlock");
			if (!WinBlock) {
				this.enabled = false;
			}
		}

		FocusObject = new GameObject("FocusObject");
		FocusObject.transform.parent = this.transform;
	}

	void LookAtTarget() {

		Vector3 focalPoint = Vector3.Lerp(
				WinBlock.transform.position, FollowTarget.transform.position, FollowDistance);
		FocusObject.transform.position = focalPoint;
		transform.LookAt(FocusObject.transform);
	}

	void Update() {

		LookAtTarget();
	}
}
