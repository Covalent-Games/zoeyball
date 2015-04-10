using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour {

	public AudioSource Audio;
	public Image MuteMusicIcon;
	public Image MuteSoundsIcon;
	bool MusicMuted = false;
	bool SoundMuted = false;

	void Awake() {

		Audio = GetComponent<AudioSource>();
	}

	public void ChangeMusicVolume(float newVolume) {

		if (!MusicMuted)
			Audio.volume = newVolume;
	}

	public void ToggleMusic() {

		MusicMuted = !MusicMuted;

		if (MusicMuted) {
			Audio.mute = false;
			MuteMusicIcon.sprite = UIResources.UISprites["MuteMusicIcon_Muted"]
					.GetComponent<SpriteRenderer>().sprite;
		} else {
			Audio.mute = true;
			MuteMusicIcon.sprite = UIResources.UISprites["MuteMusicIcon_NotMuted"]
					.GetComponent<SpriteRenderer>().sprite;
		}


	}
	public void ToggleSound() {

		SoundMuted = !SoundMuted;
		Debug.LogWarning("Muting sound effects not yet implemented!");
		if (SoundMuted) {
			MuteSoundsIcon.sprite = UIResources.UISprites["MuteSoundsIcon_Muted"]
					.GetComponent<SpriteRenderer>().sprite;
		} else {
			MuteSoundsIcon.sprite = UIResources.UISprites["MuteSoundsIcon_NotMuted"]
					.GetComponent<SpriteRenderer>().sprite;
		}
	}

}
