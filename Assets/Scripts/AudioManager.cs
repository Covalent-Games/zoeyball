using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour {

	public AudioSource[] AudioSources;
	AudioSource Audio_Music;
	AudioSource Audio_MinorClickSound;
	AudioSource Audio_MajorClickSound;
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

		Audio_Music = AudioSources[0];
		Audio_MinorClickSound = AudioSources[1];
		Audio_MajorClickSound = AudioSources[2];

		if (PlayerPrefs.HasKey("Music") && PlayerPrefs.GetString("Music") == "Off") {
			ToggleMusic();
		}
		if (PlayerPrefs.HasKey("Sound") && PlayerPrefs.GetString("Sound") == "Off") {
			ToggleSound();
		}
	}

	public void PlaySmallClickSound() {

		Audio_MinorClickSound.Play();
	}

	public void PlayLargeClickSound() {

		Audio_MajorClickSound.Play();
	}

	public void ChangeMusicVolume(float newVolume) {

		if (!MusicMuted)
			Audio_Music.volume = newVolume;

		PlayerPrefs.SetFloat("Volume", newVolume);
	}

	public void ToggleMusic() {

		if (MusicMuted) {
			Audio_Music.mute = false;
			MuteMusicImage.sprite = MusicUnMutedIcon;
			PlayerPrefs.SetString("Music", "On");
		} else {
			Audio_Music.mute = true;
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
