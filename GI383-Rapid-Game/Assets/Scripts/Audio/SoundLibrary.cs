using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewSoundLibrary", menuName = "Audio/Sound Library")]
public class SoundLibrary : ScriptableObject
{
    public SoundData[] sounds;

    private Dictionary<string, SoundData> soundDictionary;

    public void Initialize()
    {
        soundDictionary = new Dictionary<string, SoundData>();
        foreach (var sound in sounds)
        {
            if (!soundDictionary.ContainsKey(sound.soundName))
            {
                soundDictionary.Add(sound.soundName, sound);
            }
            else
            {
                Debug.LogWarning($"SoundLibrary: Duplicate sound name '{sound.soundName}' found! Ignoring duplicate.");
            }
        }
    }

    public SoundData? GetSound(string name)
    {
        if (soundDictionary == null)
        {
            Initialize();
        }

        if (soundDictionary.TryGetValue(name, out SoundData sound))
        {
            return sound;
        }

        Debug.LogWarning($"SoundLibrary: Sound '{name}' not found!");
        return null;
    }
}
