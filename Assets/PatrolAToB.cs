using UnityEngine;
using System.Collections;

public class PatrolAToB : MonoBehaviour {

	public Transform PosA;
	public Transform PosB;
	[Range(0.1f, 5.0f)]
	public float Speed;
	Transform[] Waypoints;
	int Index;

	void Awake() {

		GetWaypoints();
		//Null the parents to keep them from following the block. They're only parented for asset management.
		//TODO: Don't hardcode this.
		PosA.parent = null;
		PosB.parent = null;
		Waypoints = new Transform[] {
			PosA,
			PosB,
		};
	}

	void Update() {

		if (transform.position == Waypoints[Index % Waypoints.Length].position) {
			Index++;
		}
		transform.position = Vector3.MoveTowards(
				transform.position, Waypoints[Index % Waypoints.Length].position, Time.deltaTime * Speed);
	}

	void GetWaypoints() {

		if (PosA == null) {
			PosA = transform.FindChild("PosA");
			if (PosA == null) {
				Debug.LogError(name + " has no PosA child waypoint object");
			}
		}
		if (PosB == null) {
			PosB = transform.FindChild("PosB");
			if (PosB == null) {
				Debug.LogError(name + " has no PosB child waypoint object");
			}
		}
	}
}
