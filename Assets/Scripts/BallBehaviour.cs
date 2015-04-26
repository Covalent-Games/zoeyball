using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class BallBehaviour : MonoBehaviour {

	public GameObject FiveBounceParticle;
	Rigidbody thisRigidBody;
	GameObject[] ImpactEffects = new GameObject[3];
	int ImpactEffectPoolIndex;

	//TODO This doesn't feel like the best location for this.
	public Text ScoreText;
	public float TmpScore;
	public int TmpBounces;
	public bool StartCountingScore = false;

	void Awake() {

		thisRigidBody = GetComponent<Rigidbody>();
		GameObject resource = (GameObject)Resources.Load("Effects/ImpactEffect");
		for (int i = 0; i < ImpactEffects.Length; i++) {
			ImpactEffects[i] = Instantiate(resource) as GameObject;
		}
		UpdateScoreText();
	}

	void OnCollisionEnter(Collision obj) {

		AudioSource audioSource = obj.gameObject.GetComponent<AudioSource>();
		if (audioSource != null) {
			// Raise or lower volume of impact based on ball velocity.
			audioSource.volume = Vector3.Distance(
					(thisRigidBody.velocity * Time.deltaTime) + transform.position,
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
			TmpScore += 5;
		}

		if (TmpBounces == 5) {
			Instantiate(FiveBounceParticle, transform.position, Quaternion.identity);
		}
	}

	public void UpdateScoreText() {

		if (TmpBounces > 0) {
			ScoreText.text = string.Format("Score: {0}  Bounces: {1}",
					Mathf.RoundToInt(TmpScore),
					TmpBounces);
		} else {
			ScoreText.text = string.Format("Score: {0}", Mathf.RoundToInt(TmpScore));
		}
	}

	void Update() {

		if (StartCountingScore) {
			TmpScore += Time.deltaTime * 5;
			UpdateScoreText();
		}
	}
}
