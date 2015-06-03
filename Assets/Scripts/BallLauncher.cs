using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TransformExtensions;
using System.Collections;

public class BallLauncher : MonoBehaviour {

	public GameObject Ball;
	public AudioSource LaunchSound;
	public SpriteRenderer LaunchArrowRenderer;
	public Text LaunchButtonText;
	public Image LanchMeterImage;
	public float LaunchPowerMeter;
	public float MaxLaunchPower;
	public int RotationDirection;
	public Vector3 StartPosition;
	public Quaternion StartRotation;
	public bool CanLaunch = true;
	public string DefaultLaunchText = "Pew!";
	public string LaunchResetText = "Restart";
	bool IncreasePower = false;
	float RotateSpeed = 20f;

	void Awake() {

		Ball.GetComponent<Rigidbody>().isKinematic = true;
		LaunchArrowRenderer = transform.FindChild("LaunchArrow").GetComponent<SpriteRenderer>();
		LaunchSound = transform.FindChild("LaunchSound").GetComponent<AudioSource>();
		LaunchButtonText = transform.parent.FindChildRecursive("LaunchButtonText").GetComponent<Text>();
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
		LanchMeterImage.fillAmount = LaunchPowerMeter / MaxLaunchPower;
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
}
