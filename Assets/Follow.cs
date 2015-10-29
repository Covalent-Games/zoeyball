using UnityEngine;
using System.Collections;

public class Follow : MonoBehaviour {

	[HideInInspector]
	public GameObject FollowTarget;
	[HideInInspector]
	public GameObject WinBlock;
	[Range(0.01f, 1.0f)]
	[Tooltip("0 will stare at the Winblock. 1 will follow the ball perfectly.")]
	public float FollowDistance;
	[Tooltip("Modifies where the camera looks")]
	public Vector2 Offset;
	public bool IsShaking;
	GameObject FocusObject;

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

		if (FollowTarget) {
			Vector3 focalPoint = Vector3.Lerp(
					WinBlock.transform.position + (Vector3)Offset,
					FollowTarget.transform.position + (Vector3)Offset, FollowDistance);
			if (IsShaking) {
				var randomValue = (Vector2)SmoothRandom.GetVector3(5f);
				var scaledValue = randomValue - new Vector2(0.3f, 0.3f);
				Debug.Log("SmoothRandom value: " + randomValue.ToString());
				Debug.Log("Scaled value:" + scaledValue.ToString());
				focalPoint += Vector3.Scale(scaledValue, new Vector2(3f, 3f));
				// add to focalPoint
			}
			FocusObject.transform.position = focalPoint;
			transform.LookAt(FocusObject.transform);
		}
	}

	void Update() {

		LookAtTarget();
	}
}
