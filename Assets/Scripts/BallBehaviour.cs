using UnityEngine;
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
	public GameObject[] ImpactEffects = new GameObject[4];
	int ImpactEffectPoolIndex;
	Vector3 LastPosition;
	public GameReset ResetTrigger;

	//TODO This doesn't feel like the best location for this.
	public Text ScoreText;
	public Text BounceText;
	public float CurrentScore;
	public int CurrentBounces;
	public bool StartCountingScore = false;
	public List<Vector3> FlightPath = new List<Vector3>();

	private int _bouncePointValue = 3;
	private GameObject _plusFivePrefab;
	private List<GameObject> _plusThrees = new List<GameObject>();
	private LineRenderer _lineRenderer;

	void Awake() {

		PhysicsBody = GetComponent<Rigidbody>();
		_plusFivePrefab = (GameObject)Resources.Load("PlusThree");
		if (_plusFivePrefab == null) {
			Debug.Log("Plus five is null");
		}
		//GetComponent<Rigidbody>().angularDrag = 0f;
		_lineRenderer = GetComponent<LineRenderer>();
		if (_lineRenderer == null) {
			_lineRenderer = gameObject.AddComponent<LineRenderer>();
			GameObject go = Resources.Load("Balls/FlightPathMatObj") as GameObject;
			_lineRenderer.material = go.GetComponent<Renderer>().material;
		}
		_lineRenderer.enabled = false;
	}

	/// <summary>
	/// Triggered when this object collides with another physics object.
	/// </summary>
	/// <param name="obj"></param>
	void OnCollisionEnter(Collision obj) {

		PlayHitAudio(obj.gameObject);
		GenerateImpactEffect(obj);
		ProcessNextBouce(obj.gameObject);

	}

	/// <summary>
	/// Plays the audio from the AudioSource attached to /go/.
	/// </summary>
	/// <param name="go">The object to play the audio from</param>
	private void PlayHitAudio(GameObject go) {

		AudioSource audioSource = go.GetComponent<AudioSource>();
		if (audioSource != null) {
			// Raise or lower volume of impact based on ball velocity.
			audioSource.volume = Vector3.Distance(
					(PhysicsBody.velocity * Time.deltaTime) + transform.position,
					transform.position) * 3.3f;
			AudioSource.PlayClipAtPoint(audioSource.clip, Camera.main.transform.position);
		}
	}

	/// <summary>
	/// Generates an impact effect.
	/// </summary>
	/// <param name="collision">The Collision used to determine the location of the impact effect.</param>
	private void GenerateImpactEffect(Collision collision) {

		int index = ImpactEffectPoolIndex % ImpactEffects.Length;
		ImpactEffects[index].transform.position = collision.contacts[0].point;
		ImpactEffects[index].transform.LookAt(transform.position);
		ImpactEffects[index].GetComponent<ParticleSystem>().Play();
		ImpactEffectPoolIndex++;
	}

	/// <summary>
	/// If the provided obj is a block, increments bounces, plays audio, etc.
	/// </summary>
	/// <param name="go">The block that was hit.</param>
	private void ProcessNextBouce(GameObject go) {

		if (go.tag == "Block") {
			CurrentBounces++;
			CurrentScore += _bouncePointValue;
			StartCoroutine(PlusThreeRoutine());
			Follow follow = Camera.main.GetComponent<Follow>();

			if (CurrentBounces > 0 && CurrentBounces % 5 == 0) {
				follow.IsShaking = true;
				follow.BounceModifier = CurrentBounces / 5 + follow.BounceModifier;
				Invoke("SetShakingFlag", 0.5f);
			}

			if (CurrentBounces == 5) {
				Instantiate(FiveBounceParticle, transform.position, Quaternion.identity);
				FiveBounceTrail.GetComponent<ParticleSystem>().Play();
				AudioManager.Clips.BounceGoal_5.Play();
			} else if (CurrentBounces == 10) {
				Instantiate(TenBounceParticle, transform.position, Quaternion.identity);
				FiveBounceTrail.GetComponent<ParticleSystem>().Stop();
				TenBounceTrail.GetComponent<ParticleSystem>().Play();
				AudioManager.Clips.BounceGoal_10.Play();
			} else if (CurrentBounces == 15) {
				AudioManager.Clips.BounceGoal_15.Play();
			} else if (CurrentBounces == 20) {
				AudioManager.Clips.BounceGoal_20.Play();
			}
		}
	}

	/// <summary>
	/// Flag checked by follow.cs to determine if the camera should start shaking.
	/// </summary>
	public void SetShakingFlag() {

		Camera.main.GetComponent<Follow>().IsShaking = false;
	}

	/// <summary>
	/// Animates a '+3' to fall and fade from the score text.
	/// </summary>
	IEnumerator PlusThreeRoutine() {

		Text plusThree = null;
		float duration = Time.time + 1f;

		// Look through existing pool for an inactive object
		lock (_plusThrees) {
			for (int i = 0; i < _plusThrees.Count; i++) {
				if (!_plusThrees[i].activeSelf) {
					plusThree = _plusThrees[i].GetComponent<Text>();
					plusThree.gameObject.SetActive(true);
					break;
				}
			}
		}
		// No objects were available, so make a new one and add it to the pool
		if (plusThree == null) {
			GameObject plusThreeGo = (GameObject)Instantiate(
				_plusFivePrefab,
				ScoreText.transform.position,
				Quaternion.identity);
			_plusThrees.Add(plusThreeGo);
			plusThree = plusThreeGo.GetComponent<Text>();
			plusThree.transform.SetParent(ScoreText.transform);
		}
		Outline outline = plusThree.GetComponent<Outline>();
		Color color = plusThree.color;
		color.a = 1f;
		plusThree.color = color;
		plusThree.transform.position = ScoreText.transform.position + new Vector3(0, -25, 0);
		Vector3 pos = plusThree.transform.position + new Vector3(0, -75, 0);

		while (Time.time < duration) {
			plusThree.transform.position = Vector3.MoveTowards(
				plusThree.transform.position,
				pos,
				50 * Time.deltaTime);
			color.a = Mathf.MoveTowards(color.a, 0, Time.deltaTime);
			plusThree.color = color;
			outline.effectColor = new Color(0, 0, 0, color.a);
			yield return null;
		}
		// We're done animating, so set it inactive so we can use it again.
		plusThree.gameObject.SetActive(false);

	}

	/// <summary>
	/// CoRoutine to determine if the ball has stopped moving and should reset.
	/// </summary>
	/// <param name="lastPosition">The position from last frame.</param>
	/// <returns>IEnumerator</returns>
	IEnumerator CheckForDeadPosition(Vector3 lastPosition) {

		yield return new WaitForSeconds(.2f);
		if (transform.position == lastPosition) {
			if (!LaunchPlatform.CanLaunch) {
				GameReset.ResetLevel(gameObject.GetComponent<BallBehaviour>());
				StopAllCoroutines();
			}
		}
	}

	/// <summary>
	/// Updates the UI to reflect the current bounces and score.
	/// </summary>
	public void UpdateScoreText() {

		ScoreText.text = Mathf.RoundToInt(CurrentScore).ToString();
		BounceText.text = CurrentBounces.ToString();
	}

	/// <summary>
	/// Resets the entire level, including scores, ball position and particles.
	/// This also generates the previous flight path.
	/// </summary>
	public void Reset() {

		Camera.main.GetComponent<Follow>().IsShaking = false;

		FiveBounceTrail.GetComponent<ParticleSystem>().Stop();
		FiveBounceTrail.GetComponent<ParticleSystem>().Clear();
		TenBounceTrail.GetComponent<ParticleSystem>().Stop();
		TenBounceTrail.GetComponent<ParticleSystem>().Clear();

		transform.position = LaunchPlatform.transform.position;
		transform.rotation = LaunchPlatform.transform.rotation;

		PhysicsBody.isKinematic = true;
		PhysicsBody.velocity = Vector3.zero;

		CurrentScore = 0f;
		CurrentBounces = 0;
		StartCountingScore = false;
		UpdateScoreText();
		DrawPreviousFlightPath();
	}

	/// <summary>
	/// Draws a path indicating where the ball traveled on the last attempt.
	/// </summary>
	/// <param name="clearPath">bool: true will clear the previous flight path position list.</param>
	public void DrawPreviousFlightPath(bool clearPath = true) {

		_lineRenderer.enabled = true;
		_lineRenderer.SetVertexCount(FlightPath.Count);
		_lineRenderer.SetColors(Color.blue, Color.blue);
		_lineRenderer.SetWidth(0.1f, 0.1f);

		for (int i = 0; i < FlightPath.Count; i++) {
			_lineRenderer.SetPosition(i, FlightPath[i]);
		}
		if (clearPath)
			FlightPath.Clear();
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
				//TODO: Monitor this. If it gets too expensive limit iterations via coroutine.
				FlightPath.Add(transform.position);
			}
		}
	}
}
