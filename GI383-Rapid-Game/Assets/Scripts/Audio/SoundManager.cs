using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private SoundLibrary soundLibrary;
    [SerializeField] private AudioSource sfxSourcePrefab; // Optional: Prefab for better control, or we generate them

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (soundLibrary != null)
        {
            soundLibrary.Initialize();
        }
    }

    /// <summary>
    /// Plays a sound at the camera's position (2D non-spatial).
    /// </summary>
    public void PlaySound(string soundName)
    {
        // For 2D non-spatial sounds, we can just play at the listener's position
        // or use a dedicated 2D AudioSource on the manager itself.
        // For simplicity in this request, we'll treat it as "At positions" where position is camera/listener.
        if (Camera.main != null)
        {
            PlaySound(soundName, Camera.main.transform.position);
        }
    }

    /// <summary>
    /// Plays a sound at a specific 2D position.
    /// </summary>
    public void PlaySound(string soundName, Vector2 position)
    {
        if (soundLibrary == null)
        {
            Debug.LogWarning("SoundManager: SoundLibrary is missing!");
            return;
        }

        SoundData? data = soundLibrary.GetSound(soundName);
        if (data == null) return;
        
        SoundData sound = data.Value;

        // Create a temporary GameObject for the sound
        GameObject soundObj = new GameObject("TempAudio_" + soundName);
        soundObj.transform.position = position;

        AudioSource source = soundObj.AddComponent<AudioSource>();
        source.clip = sound.clip;
        source.volume = sound.volume;
        
        // Randomize pitch
        if (sound.pitchVariance > 0f)
        {
            source.pitch = sound.pitch + Random.Range(-sound.pitchVariance, sound.pitchVariance);
        }
        else
        {
            source.pitch = sound.pitch;
        }

        // Play and Destroy
        source.Play();
        Destroy(soundObj, sound.clip.length / source.pitch); // Adjust destroy time for pitch
    }
}
