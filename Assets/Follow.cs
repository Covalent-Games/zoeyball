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
	private Vector3 _shakeRange = new Vector3(.2f, .2f, .2f);
	private int _shakeSpeed = 50;

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
				Debug.Log("Shake speed: " + _shakeSpeed.ToString());
				var randomValue = (Vector2)SmoothRandom.GetVector3(_shakeSpeed--);
				var scaledValue = randomValue;// -new Vector2(0.3f, 0.3f);
				Debug.Log("SmoothRandom value: " + randomValue.ToString());
				//Debug.Log("Scaled value:" + scaledValue.ToString());
				focalPoint += Vector3.Scale(scaledValue, Vector3.Scale(_shakeRange, new Vector3(BounceModifier, BounceModifier, BounceModifier)));
				_shakeSpeed *= -1;
				_shakeRange = new Vector3(_shakeRange.x * -1, _shakeRange.y);
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
