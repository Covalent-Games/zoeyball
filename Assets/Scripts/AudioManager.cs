using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections;

public class AudioManager : MonoBehaviour {

	public struct Clips {
		public static AudioSource Click_light;
		public static AudioSource Click_heavy;
		public static AudioSource BounceGoal_5;
		public static AudioSource BounceGoal_10;
		public static AudioSource BounceGoal_15;
		public static AudioSource BounceGoal_20;
	}
	AudioSource Audio_Music = new AudioSource();
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

		InstantiateAudioSources();
		LoadAudioClips();
		AllocateMixerGroups();

	}

	private void LoadAudioClips() {

		string path = "Audio/SFX/";
		Clips.Click_light.clip = (AudioClip)Resources.Load(path + "SoftClick_01");
		Clips.Click_heavy.clip = (AudioClip)Resources.Load(path + "SoftClick_02");
		Clips.BounceGoal_5.clip = (AudioClip)Resources.Load(path + "5BounceSound");
		Clips.BounceGoal_10.clip = (AudioClip)Resources.Load(path + "10BounceSound");
		Clips.BounceGoal_15.clip = (AudioClip)Resources.Load(path + "15BounceSound");
		Clips.BounceGoal_20.clip = (AudioClip)Resources.Load(path + "20BounceSound");
		Audio_Music.clip = (AudioClip)Resources.Load("Audio/Music/ZoeyBallTheme");
	}

	private void AllocateMixerGroups() {

		AudioMixerGroup soundsGroup = Mixer.FindMatchingGroups("Sounds")[0];
		Debug.Log(soundsGroup.name);
		Clips.Click_light.outputAudioMixerGroup = soundsGroup;
		Clips.Click_heavy.outputAudioMixerGroup = soundsGroup;
		Clips.BounceGoal_5.outputAudioMixerGroup = soundsGroup;
		Clips.BounceGoal_10.outputAudioMixerGroup = soundsGroup;
		Clips.BounceGoal_15.outputAudioMixerGroup = soundsGroup;
		Clips.BounceGoal_20.outputAudioMixerGroup = soundsGroup;
	}


	void InstantiateAudioSources() {

		Audio_Music = gameObject.AddComponent<AudioSource>();
		Audio_Music.loop = true;
		Clips.Click_light = gameObject.AddComponent<AudioSource>();
		Clips.Click_heavy = gameObject.AddComponent<AudioSource>();
		Clips.BounceGoal_5 = gameObject.AddComponent<AudioSource>();
		Clips.BounceGoal_10 = gameObject.AddComponent<AudioSource>();
		Clips.BounceGoal_15 = gameObject.AddComponent<AudioSource>();
		Clips.BounceGoal_20 = gameObject.AddComponent<AudioSource>();
	}

	void Start() {

		Audio_Music.Play();
		if (PlayerPrefs.HasKey("Music") && PlayerPrefs.GetString("Music") == "Off") {
			ToggleMusic();
		}
		if (PlayerPrefs.HasKey("Sound") && PlayerPrefs.GetString("Sound") == "Off") {
			ToggleSound();
		}
	}

	public void PlaySmallClickSound() {

		Clips.Click_light.Play();
	}

	public void PlayLargeClickSound() {

		Debug.Log(Clips.Click_heavy);
		Clips.Click_heavy.Play();
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

		if (SoundMuted) {
			Mixer.SetFloat("SoundVolume", 0f);
			MuteSoundsImage.sprite = SoundUnmutedIcon;
			PlayerPrefs.SetString("Sound", "On");
		} else {
			Mixer.SetFloat("SoundVolume", -80f);
			MuteSoundsImage.sprite = SoundMutedIcon;
			PlayerPrefs.SetString("Sound", "Off");
		}

		SoundMuted = !SoundMuted;
	}

}
