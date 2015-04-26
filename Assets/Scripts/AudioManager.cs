using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour {

	public AudioSource Audio;
	public Image MuteMusicImage;
	public Image MuteSoundsImage;
	public Sprite MusicMutedIcon;
	public Sprite MusicUnMutedIcon;
	public Sprite SoundMutedIcon;
	public Sprite SoundUnmutedIcon;
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

		if (MusicMuted) {
			Audio.mute = false;
			MuteMusicImage.sprite = MusicUnMutedIcon;
		} else {
			Audio.mute = true;
			MuteMusicImage.sprite = MusicMutedIcon;
		}

		MusicMuted = !MusicMuted;
	}

	public void ToggleSound() {

		SoundMuted = !SoundMuted;
		Debug.LogWarning("Muting sound effects not yet implemented!");
		if (SoundMuted) {
			MuteSoundsImage.sprite = SoundMutedIcon;
		} else {
			MuteSoundsImage.sprite = SoundUnmutedIcon;
		}
	}

}
