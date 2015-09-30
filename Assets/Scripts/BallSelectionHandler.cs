using UnityEngine;
using System.Collections;
using GameData;

public class BallSelectionHandler : MonoBehaviour {

	private int _currentChildIndex = 0;
	[SerializeField]
	private GameObject _spawnLocationGO;
	private bool _ballSelected = false;
	private GameObject _selectedBall;

	void Awake() {

		if (_spawnLocationGO == null) {
			Transform t = transform.FindChild("BallSpawnObject");
			if (t != null) {
				_spawnLocationGO = t.gameObject;
			} else {
				Debug.LogError("BallSpawnObject isn't in BallSelection scene, or is not parented to BallContainer.");
			}
		}
	}

	void Start() {

		StartCoroutine(StartBallSpawnRoutine());
	}

	IEnumerator StartBallSpawnRoutine() {

		GameObject ball;
		Rigidbody rb;
		foreach (var entry in DataManager.BallNamePathPairs) {
			yield return new WaitForSeconds(0.2f);
			if (DataManager.SaveData.UnlockedBalls.ContainsKey(entry.Key)) {
				ball = (GameObject)Instantiate(
					Resources.Load(entry.Value));
				ball.name = ball.name.Replace("(Clone)", "").Trim();
				// Disable points/timer etc
				ball.GetComponent<BallBehaviour>().enabled = false;
				// Enable physics
				rb = ball.GetComponent<Rigidbody>();
				rb.isKinematic = false;
				rb.angularDrag = 0.1f;
				rb.velocity = Vector3.Normalize(new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 0f), 0)) * 5;
				ball.transform.position = _spawnLocationGO.transform.position;
				ball.transform.parent = transform;
			}
		}
	}

	public void ScrollRight() {

		if (_currentChildIndex + 1 < transform.childCount) {
			_currentChildIndex++;
		}
	}

	public void ScrollLeft() {

		if (_currentChildIndex > 0) {
			_currentChildIndex--;
		}
	}

	public void SelectBall() {

		Debug.Log("Previous ball: " + GameManager.SelectedBall.name);
		GameManager.SelectedBall = (GameObject)Resources.Load(DataManager.BallNamePathPairs[_selectedBall.name]);
		Debug.Log("New ball: " + GameManager.SelectedBall.name);

		Instantiate(_selectedBall.GetComponent<BallBehaviour>().FiveBounceParticle,
			_selectedBall.transform.position,
			Quaternion.identity);
		DataManager.SaveData.CurrentSelectedBallName = _selectedBall.name;
		PlayerPrefs.SetString("CurrentBall", _selectedBall.name);

		Invoke("LoadLevelPicker", 1.5f);
	}

	void LoadLevelPicker() {

		Application.LoadLevel(1);
	}

	void FixedUpdate() {

		if (Input.touchCount > 0) {
			for (int i = 0; i < Input.touchCount; i++) {
				Touch touch = Input.GetTouch(i);
				if (touch.phase == TouchPhase.Began) {
					if (_ballSelected) {
						if (!ConfirmBallSelection(touch.position)) {
							DeselectBall();
						}
					} else {
						HighlightNewBall(touch.position);
					}
				}
			}
		}
		//if (transform == null) {
		//	Debug.Log("Transform is null!!!! HUH!?!?!");
		//} else if (transform.childCount == 0) {
		//	Debug.Log("No children");
		//} else {
		//	Transform child = transform.GetChild(CurrentChildIndex);
		//	if (child != null) {
		//		Vector3 moveTo = transform.GetChild(CurrentChildIndex).transform.position;
		//		moveTo.z = Camera.main.transform.position.z;

		//		Camera.main.transform.position = Vector3.Slerp(
		//			Camera.main.transform.position,
		//			moveTo,
		//			10 * Time.deltaTime);
		//	}
		//}
	}

	private void HighlightNewBall(Vector2 touchPos) {

		RaycastHit hitinfo;
		if (Physics.Raycast(Camera.main.ScreenPointToRay(touchPos), out hitinfo, 100f)) {
			if (hitinfo.transform.CompareTag("Ball")) {
				_selectedBall = hitinfo.transform.gameObject;
				_ballSelected = true;
				hitinfo.transform.GetComponent<Rigidbody>().isKinematic = true;
				// Move ball to "front and center".
				//TODO: This should "animate" and make sparks and be awesome.
				hitinfo.transform.position = new Vector3(0f, 3.5f, -7f);
			}
		}
	}

	private bool ConfirmBallSelection(Vector2 touchPos) {

		RaycastHit hitinfo;
		if (Physics.Raycast(Camera.main.ScreenPointToRay(touchPos), out hitinfo, 100f)) {
			// Ball tapped, and it was the previously selected ball
			if (hitinfo.transform.name == _selectedBall.name) {
				SelectBall();
				return true;
			} else {
				// Screen tapped, object selected, but it wasn't the selected ball
				return false;
			}
		} else {
			// Screen tapped, but nothing selected.
			return false;
		}
	}

	private void DeselectBall() {

		_selectedBall.transform.position = new Vector3(0f, 3.5f, 0f);
		_selectedBall.GetComponent<Rigidbody>().isKinematic = false;
		_ballSelected = false;
		_selectedBall = null;
	}
}
