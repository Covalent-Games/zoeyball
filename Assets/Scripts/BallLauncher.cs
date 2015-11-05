﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TransformExtensions;
using System.Collections;

public class BallLauncher : MonoBehaviour {

	public static GameObject Ball;
	public AudioSource LaunchSound;
	public SpriteRenderer LaunchArrowRenderer;
	public Text LaunchButtonText;
	public Image LaunchMeterImage;
	public Image PreviousPower;
	public float LaunchPowerMeter;
	public float MaxLaunchPower;
	public int RotationDirection;
	public Vector3 StartPosition;
	public Quaternion StartRotation;
	public bool CanLaunch = true;
	public string DefaultLaunchText = "Hold to launch";
	public string LaunchResetText = "Restart";
	bool IncreasePower = false;
	float RotateSpeed = 20f;

	void Awake() {


		LaunchArrowRenderer = transform.FindChild("LaunchArrow").GetComponent<SpriteRenderer>();
		LaunchSound = transform.FindChild("LaunchSound").GetComponent<AudioSource>();
		LaunchMeterImage = GameObject.Find("LaunchBar").GetComponent<Image>();
		PreviousPower = GameObject.Find("PreviousPowerIndicator").GetComponent<Image>();
		LaunchButtonText = transform.parent.FindChildRecursive("LaunchButtonText").GetComponent<Text>();
		LoadSelectedBall();
	}

	void LoadSelectedBall() {

		Ball = transform.GetChild(0).gameObject;
		if (!Ball) {
			Debug.Log("Ball is null");
		}
		if (!Application.isEditor) {
			// Disable old ball's collider to prevent random collision effects.
			Debug.Log("Enabling SphereCollider");
			BallLauncher.Ball.GetComponent<SphereCollider>().enabled = false;
			// Create the new, selected ball.
			Debug.Log("Attempting to instantiate " + GameManager.SelectedBall);
			GameObject ball = Instantiate(GameManager.SelectedBall);
			Debug.Log("Setting " + ball + "'s position and rotation");
			ball.transform.position = BallLauncher.Ball.transform.position;
			ball.transform.rotation = BallLauncher.Ball.transform.rotation;
			// Get rid of the default ball.
			Destroy(BallLauncher.Ball.gameObject);
			BallLauncher.Ball = ball;
			ball.transform.parent = transform;
		}
		if (GameManager.SelectedBall != null) {
			Debug.Log("GameManager.SelectedBall = " + GameManager.SelectedBall.name);
		}
		BallBehaviour bb = Ball.GetComponent<BallBehaviour>();
		bb.ScoreText = transform.parent.FindChildRecursive("ScoreText").GetComponent<Text>();
		bb.BounceText = transform.parent.FindChildRecursive("BounceText").GetComponent<Text>();
		bb.LaunchPlatform = this;
		bb.PhysicsBody.isKinematic = true;
		GameObject resource = (GameObject)Resources.Load("Effects/ImpactEffect");
		for (int i = 0; i < bb.ImpactEffects.Length; i++) {
			bb.ImpactEffects[i] = Instantiate(resource) as GameObject;
		}
		Camera.main.gameObject.GetComponent<Follow>().FollowTarget = BallLauncher.Ball;
	}

	void FixedUpdate() {

		// If the ball is in flight, effectively disable control button functionality.
		if (!CanLaunch) { return; }

		if (IncreasePower)
			IncreasePowerMeter();

		if (RotationDirection != 0) {
			RotateLauncher(RotationDirection);
		}
	}

	public void ApplyRotationDirection(int direction) {

		if (direction == 0) {
			RotateSpeed = 20f;
		}
		RotationDirection = direction;
	}

	public void RotateLauncher(float input) {

		Vector3 rotation = transform.rotation.eulerAngles;
		Vector3 newRotation = new Vector3(rotation.x, rotation.y, rotation.z + (-input * 2.5f));
		RotateSpeed += 2f;
		transform.rotation = Quaternion.Euler(
				Vector3.MoveTowards(
						transform.rotation.eulerAngles, newRotation, Time.deltaTime * RotateSpeed));

	}

	public void TogglePowerIncrease() {

		IncreasePower = !IncreasePower;

		// Increase power is false when the user lifts there finger off the button
		if (!IncreasePower)
			LaunchBall();
		// If the ball is in flight, and the launch button is pressed, reset the level.
		if (IncreasePower && !CanLaunch)
#if UNITY_EDITOR
			if (GameManager.Instance == null)
				Application.LoadLevel(Application.loadedLevel);
			else
				GameManager.Instance.Restart();
#else
			GameManager.Instance.Restart();
#endif
	}

	void IncreasePowerMeter() {

		LaunchPowerMeter = Mathf.MoveTowards(LaunchPowerMeter, MaxLaunchPower, Time.deltaTime * 3);
		LaunchMeterImage.fillAmount = LaunchPowerMeter / MaxLaunchPower;
	}

	public void LaunchBall() {

		if (CanLaunch) {
			LaunchSound.Play();
			LaunchArrowRenderer.enabled = false;
			Rigidbody ball = Ball.GetComponent<Rigidbody>();

			StartPosition = Ball.transform.position;
			StartRotation = Ball.transform.rotation;
			ball.isKinematic = false;
			ball.AddForce(Ball.transform.up * LaunchPowerMeter * 100);
			ball.GetComponent<BallBehaviour>().StartCountingScore = true;
			CanLaunch = false;
			LaunchButtonText.text = LaunchResetText;
		}
	}

	public void Reset() {

		SetPreviousPowerIndicator();
		LaunchPowerMeter = 0f;
		LaunchButtonText.text = DefaultLaunchText;
		LaunchArrowRenderer.enabled = true;
	}

	internal void SetPreviousPowerIndicator() {
		Vector3 pos = PreviousPower.transform.localPosition;
		float meterWidth = 0;
		if (RestartCache.LoadFromCache)
			meterWidth = RestartCache.MeterWidth;
		else
			meterWidth = LaunchMeterImage.rectTransform.rect.width;

		pos.x = (LaunchMeterImage.fillAmount * meterWidth) - (meterWidth / 2f);
		PreviousPower.transform.localPosition = pos;
	}
}
