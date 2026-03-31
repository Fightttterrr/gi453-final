using UnityEngine;

[System.Serializable]
public struct SoundData
{
    public string soundName;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume;
    [Range(0.1f, 3f)]
    public float pitch;
    [Range(0f, 1f)]
    public float pitchVariance;

    // Constructor for default values
    public SoundData(string name, AudioClip audioClip)
    {
        soundName = name;
        clip = audioClip;
        volume = 1f;
        pitch = 1f;
        pitchVariance = 0f;
    }
}
