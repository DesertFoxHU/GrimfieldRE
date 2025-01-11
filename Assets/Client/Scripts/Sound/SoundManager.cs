using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    public List<SoundSerialize> registerSounds;
    public Dictionary<string, AudioClip> sounds = new Dictionary<string, AudioClip>();

    private AudioSource source;

    public void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        Instance = this;
        source = GetComponent<AudioSource>();
        foreach(SoundSerialize sound in registerSounds)
        {
            Debug.Log($"{sound}");
            sounds.Add(sound.sound.ToString(), sound.clip);
        }
    }

    public static void PlaySound(Sound sound)
    {
        PlaySound(sound.ToString());
    }

    public static void PlaySound(string sound)
    {
        if (!Instance.sounds.ContainsKey(sound))
        {
            Debug.LogError($"Not found AudioClip named {sound}");
            return;
        }

        Instance.source.PlayOneShot(Instance.sounds[sound]);
    }
}

public enum Sound
{
    YourTurn,
    UniversalButtonClick,
}

[System.Serializable]
public struct SoundSerialize
{
    public Sound sound;
    public AudioClip clip;
}
