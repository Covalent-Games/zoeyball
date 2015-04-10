using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BallLauncher : MonoBehaviour {

	public GameObject Ball;
	public AudioSource LaunchSound;
	public Image LanchMeterImage;
	public float LaunchPowerMeter;
	public float MaxLaunchPower;
	public int RotationDirection;
	public Vector3 StartPosition;
	public Quaternion StartRotation;
	public bool CanLaunch = true;
	bool IncreasePower = false;
	float RotateSpeed = 20f;

	void Awake() {

		Ball.GetComponent<Rigidbody>().isKinematic = true;
		LaunchSound = transform.FindChild("LaunchSound").GetComponent<AudioSource>();
	}

	void FixedUpdate() {

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
		Vector3 newRotation = new Vector3(rotation.x, rotation.y, rotation.z + (-input * 2));
		RotateSpeed += 1f;
		transform.rotation = Quaternion.Euler(
				Vector3.MoveTowards(
						transform.rotation.eulerAngles, newRotation, Time.deltaTime * RotateSpeed));

	}

	public void TogglePowerIncrease() {

		IncreasePower = !IncreasePower;

		if (!IncreasePower)
			LaunchBall();
	}

	void IncreasePowerMeter() {

		LaunchPowerMeter = Mathf.MoveTowards(LaunchPowerMeter, MaxLaunchPower, Time.deltaTime * 3);
		LanchMeterImage.fillAmount = LaunchPowerMeter / MaxLaunchPower;
	}

	public void LaunchBall() {

		if (CanLaunch) {
			LaunchSound.Play();
			Rigidbody ball = Ball.GetComponent<Rigidbody>();

			StartPosition = Ball.transform.position;
			StartRotation = Ball.transform.rotation;
			ball.isKinematic = false;
			ball.AddForce(Ball.transform.up * LaunchPowerMeter * 100);
			ball.GetComponent<BallBehaviour>().StartCountingScore = true;
			CanLaunch = false;
		}
	}
}
