﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TransformExtensions;
using System.Collections;
using System.Collections.Generic;

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
	public float CurrentScore;
	public int CurrentBounces;
	public bool StartCountingScore = false;

	private GameObject _plusFivePrefab;
	private List<GameObject> _plusFives = new List<GameObject>();

	void Awake() {

		_plusFivePrefab = (GameObject)Resources.Load("PlusFive");
		GetComponent<Rigidbody>().angularDrag = 0f;
	}

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
			CurrentBounces++;
			CurrentScore += 5;
			StartCoroutine(PlusFiveRoutine());
		}

		if (CurrentBounces == 5) {
			Instantiate(FiveBounceParticle, transform.position, Quaternion.identity);
			FiveBounceTrail.GetComponent<ParticleSystem>().Play();
		} else if (CurrentBounces == 10) {
			Instantiate(TenBounceParticle, transform.position, Quaternion.identity);
			FiveBounceTrail.GetComponent<ParticleSystem>().Stop();
			TenBounceTrail.GetComponent<ParticleSystem>().Play();
		} else if (CurrentBounces == 15) {

		}
	}

	/// <summary>
	/// Animates a '+5' to fall and fade from the score text.
	/// </summary>
	IEnumerator PlusFiveRoutine() {

		Text plusFive = null;
		float duration = Time.time + 1f;

		// Look through existing pool for an inactive object
		lock (_plusFives) {
			for (int i = 0; i < _plusFives.Count; i++) {
				if (_plusFives[i].activeSelf) {
					plusFive = _plusFives[i].GetComponent<Text>();
					plusFive.gameObject.SetActive(true);
					break;
				}
			}
		}
		// No objects were available, so make a new one and add it to the pool
		if (plusFive == null) {
			GameObject plusFiveGO = (GameObject)Instantiate(
				_plusFivePrefab,
				ScoreText.transform.position,
				Quaternion.identity);
			_plusFives.Add(plusFiveGO);
			plusFive = plusFiveGO.GetComponent<Text>();
			plusFive.transform.SetParent(ScoreText.transform);
		}
		Outline outline = plusFive.GetComponent<Outline>();
		Color color = plusFive.color;
		color.a = 1f;
		plusFive.color = color;
		plusFive.transform.position = ScoreText.transform.position + new Vector3(0, -25, 0);
		Vector3 pos = plusFive.transform.position + new Vector3(0, -75, 0);

		while (Time.time < duration) {
			plusFive.transform.position = Vector3.MoveTowards(
				plusFive.transform.position,
				pos,
				50 * Time.deltaTime);
			color.a = Mathf.MoveTowards(color.a, 0, Time.deltaTime);
			plusFive.color = color;
			outline.effectColor = new Color(0, 0, 0, color.a);
			yield return null;
		}
		// We're done animating, so set it inactive so we can use it again.
		plusFive.gameObject.SetActive(false);

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

		ScoreText.text = Mathf.RoundToInt(CurrentScore).ToString();
		BounceText.text = CurrentBounces.ToString();
	}

	void Update() {

		if (StartCountingScore) {
			CurrentScore += Time.deltaTime * 5;
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
