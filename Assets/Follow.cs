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
	public float BounceModifier;
	GameObject FocusObject;

	private static int _flipper = 1;
	private Vector3 _shakeRange = new Vector3(1f, 1f, 1f);
	private int _shakeSpeed = 10;

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

		StartCoroutine(LookAtTarget());
	}

	IEnumerator LookAtTarget() {
		Vector2? initialRandomValue = null;	// Captures first random value to set a median.

		while (true) {
			if (FollowTarget) {
				Vector3 focalPoint = Vector3.Lerp(
						WinBlock.transform.position + (Vector3)Offset,
						FollowTarget.transform.position + (Vector3)Offset, FollowDistance);
				if (IsShaking) {
					if (initialRandomValue == null)
						initialRandomValue = (Vector2)SmoothRandom.GetVector3(_shakeSpeed);
					Vector2 scaledValue = (Vector2)SmoothRandom.GetVector3(_shakeSpeed);
					focalPoint += Vector3.Scale(scaledValue - initialRandomValue.Value, Vector3.Scale(_shakeRange, new Vector3(BounceModifier, BounceModifier, BounceModifier)));
				} else if (initialRandomValue != null) {
					initialRandomValue = null;
				}
				FocusObject.transform.position = focalPoint;
				transform.LookAt(FocusObject.transform);
			}

			yield return new WaitForFixedUpdate();
		}
	}
}
