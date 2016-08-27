using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Sound : MonoBehaviour {
    static public Sound I { get { return instance; } }
    static Sound instance;
    public float masterVolume = 0.1f;

    public Dictionary<int, AudioSource> channels = new Dictionary<int, AudioSource>();
    GameObject channelBox;

    void Awake() {
        instance = this;
        channelBox = new GameObject();
        channelBox.name = "BGMChannel";
        channelBox.transform.SetParent(transform);
    }

    public void AddClip(string clipName, int channelID) {
        AudioClip addClip;
        addClip = Resources.Load<AudioClip>(clipName);
        AddClip(addClip, channelID);
    }

    public void AddClip(AudioClip addClip, int channelID) {
        GameObject tmp = new GameObject();
        tmp.AddComponent<AudioSource>();
        tmp.GetComponent<AudioSource>().clip = addClip;
        tmp.name = addClip.name;
        tmp.transform.SetParent(channelBox.transform);
        channels.Add(channelID, tmp.GetComponent<AudioSource>());
    }

    public void PlayBGM() {
        StartCoroutine("Playing");
	}

    IEnumerator Playing(){
        foreach (AudioSource channel in channels.Values) {
            channel.Play();
        }
        while (true) {
            foreach (AudioSource channel in channels.Values) {
                if (!channel.isPlaying) {
                    foreach (AudioSource channelB in channels.Values) {
                        channelB.Play();
                    }
                    StartCoroutine("Playing");
                    yield break;
                }
            }
            yield return null;
        }
    }

    public void SetChannelVolume(int channelID, float volume) {
        channels[channelID].volume = volume * masterVolume;
    }

    public void FadeChannelVolume(int channelID, float volume) {
        StartCoroutine(IEnumFadeChannelVolume(channelID, volume * masterVolume));
    }

    IEnumerator IEnumFadeChannelVolume(int channelID, float volume) {
        float beginVolume = channels[channelID].volume;
        float diff = volume * masterVolume - beginVolume;
        for (int i = 0; i < 120; ++i) {
            channels[channelID].volume = beginVolume + (diff * i) / 120.0f;
            yield return null;
        }
    }

    public void PlaySound(string soundName) {
        StartCoroutine("PlaySE", soundName);
    }

    IEnumerator PlaySE(string soundName) {
        GameObject tmp = new GameObject();
        tmp.AddComponent<AudioSource>();
        tmp.GetComponent<AudioSource>().clip = Resources.Load<AudioClip>("Sound/" + soundName);
        tmp.GetComponent<AudioSource>().Play();
		tmp.GetComponent<AudioSource>().volume = masterVolume;
        while (true) {
            if (!tmp.GetComponent<AudioSource>().isPlaying) {
                Destroy(tmp);
                yield break;
            }
            yield return null;
        }
    }
}
