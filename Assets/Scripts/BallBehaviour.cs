using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class BallBehaviour : MonoBehaviour {

	Rigidbody _ThisRigidBody;
	GameObject[] ImpactEffects = new GameObject[3];
	int ImpactEffectPoolIndex;

	//TODO This doesn't feel like the best location for this.
	public Text ScoreText;
	public float TmpScore;
	public int TmpBounces;
	public bool StartCountingScore = false;

	void Awake() {

		_ThisRigidBody = GetComponent<Rigidbody>();
		GameObject resource = (GameObject)Resources.Load("Effects/ImpactEffect");
		for (int i = 0; i < ImpactEffects.Length; i++) {
			ImpactEffects[i] = Instantiate(resource) as GameObject;
		}
		UpdateScoreText(0);
	}

	void OnCollisionEnter(Collision obj) {

		AudioSource audioSource = obj.gameObject.GetComponent<AudioSource>();
		if (audioSource != null) {
			// Raise or lower volume of impact based on ball velocity.
			audioSource.volume = Vector3.Distance(
					(_ThisRigidBody.velocity * Time.deltaTime) + transform.position,
					transform.position) * 3.3f;
			audioSource.Play();
		}

		int index = ImpactEffectPoolIndex % ImpactEffects.Length;
		ImpactEffects[index].transform.position = obj.contacts[0].point;
		ImpactEffects[index].transform.LookAt(transform.position);
		ImpactEffects[index].GetComponent<ParticleSystem>().Play();
		ImpactEffectPoolIndex++;

		if (obj.gameObject.tag == "Block") {
			TmpBounces++;
		}
	}

	float CalculateScoreThisUpdate(out float multiplier) {

		int bounces = TmpBounces;
		multiplier = 1 + ((bounces > 0) ? bounces / 2f : 0);

		//return Vector3.Distance(transform.position, transform.position + _ThisRigidBody.velocity)
		//		* Time.deltaTime
		//		* multiplier;
		return Time.deltaTime * multiplier * 3;
	}

	public void UpdateScoreText(float multiplier) {

		if (multiplier > 1) {
			ScoreText.text = string.Format("Score: {0}  Bounces: {1}",
					Mathf.RoundToInt(TmpScore),
					TmpBounces);
		} else {
			ScoreText.text = string.Format("Score: {0}", Mathf.RoundToInt(TmpScore));
		}
	}

	void Update() {

		float multiplier;
		if (StartCountingScore) {
			TmpScore += CalculateScoreThisUpdate(out multiplier);
			UpdateScoreText(multiplier);
		}
	}
}
