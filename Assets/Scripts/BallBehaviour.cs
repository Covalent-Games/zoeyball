using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TransformExtensions;
using System.Collections;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class BallBehaviour : MonoBehaviour {

	public GameObject FiveBounceParticle;
	public GameObject FiveBounceTrail;
	public GameObject TenBounceParticle;
	public GameObject TenBounceTrail;
	public GameObject FifteenBounceParticle;
	public BallLauncher LaunchPlatform;
	public Rigidbody PhysicsBody;
	public GameObject[] ImpactEffects = new GameObject[3];
	int ImpactEffectPoolIndex;
	Vector3 LastPosition;
	public GameReset ResetTrigger;

	//TODO This doesn't feel like the best location for this.
	public Text ScoreText;
	public Text BounceText;
	public float TmpScore;
	public int TmpBounces;
	public bool StartCountingScore = false;

	void OnCollisionEnter(Collision obj) {

		AudioSource audioSource = obj.gameObject.GetComponent<AudioSource>();
		if (audioSource != null) {
			// Raise or lower volume of impact based on ball velocity.
			audioSource.volume = Vector3.Distance(
					(PhysicsBody.velocity * Time.deltaTime) + transform.position,
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
			FiveBounceTrail.GetComponent<ParticleSystem>().Play();
		} else if (TmpBounces == 10) {
			Instantiate(TenBounceParticle, transform.position, Quaternion.identity);
			FiveBounceTrail.GetComponent<ParticleSystem>().Stop();
			TenBounceTrail.GetComponent<ParticleSystem>().Play();
		} else if (TmpBounces == 15) {

		} else {
		}
	}

	IEnumerator CheckForDeadPosition(Vector3 lastPosition) {

		yield return new WaitForSeconds(.2f);
		if (transform.position == lastPosition) {
			if (!LaunchPlatform.CanLaunch) {
				GameReset.ResetLevel(gameObject);
				StopAllCoroutines();
			}
		}
	}

	public void UpdateScoreText() {

		ScoreText.text = Mathf.RoundToInt(TmpScore).ToString();
		BounceText.text = TmpBounces.ToString();
	}

	void Update() {

		if (StartCountingScore) {
			TmpScore += Time.deltaTime * 5;
			UpdateScoreText();
		}


	}

	void FixedUpdate() {

		if (!LaunchPlatform.CanLaunch) {
			if (LastPosition == transform.position) {
				StartCoroutine(CheckForDeadPosition(LastPosition));
			} else {
				LastPosition = transform.position;
			}
		}
	}
}
