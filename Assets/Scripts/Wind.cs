using UnityEngine;
using System.Collections;

public class Wind : MonoBehaviour {

	[Range(10, 100)]
	public int Strength;

	void OnTriggerStay(Collider obj) {

		if (obj.tag == "Ball") {
			obj.GetComponent<Rigidbody>().AddForce(transform.up * Strength);
		}
	}
}
