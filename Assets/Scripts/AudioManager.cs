using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour {

	public AudioSource Audio;
	public AudioMixer Mixer;
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

		if (PlayerPrefs.HasKey("Music") && PlayerPrefs.GetString("Music") == "Off") {
			ToggleMusic();
		}
		if (PlayerPrefs.HasKey("Sound") && PlayerPrefs.GetString("Sound") == "Off") {
			ToggleSound();
		}
	}

	void Start() {


	}

	public void ChangeMusicVolume(float newVolume) {

		if (!MusicMuted)
			Audio.volume = newVolume;

		PlayerPrefs.SetFloat("Volume", newVolume);
	}

	public void ToggleMusic() {

		if (MusicMuted) {
			Audio.mute = false;
			MuteMusicImage.sprite = MusicUnMutedIcon;
			PlayerPrefs.SetString("Music", "On");
		} else {
			Audio.mute = true;
			MuteMusicImage.sprite = MusicMutedIcon;
			PlayerPrefs.SetString("Music", "Off");
		}

		MusicMuted = !MusicMuted;
	}

	public void ToggleSound() {

		SoundMuted = !SoundMuted;
		Debug.LogWarning("Muting sound effects not yet implemented!");
		if (SoundMuted) {
			Mixer.SetFloat("SoundVolume", -80f);
			MuteSoundsImage.sprite = SoundMutedIcon;
			PlayerPrefs.SetString("Sound", "Off");
		} else {
			Mixer.SetFloat("SoundVolume", 0f);
			MuteSoundsImage.sprite = SoundUnmutedIcon;
			PlayerPrefs.SetString("Sound", "On");
		}
	}

}
