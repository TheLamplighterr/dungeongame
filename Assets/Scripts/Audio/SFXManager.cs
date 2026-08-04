using UnityEngine;

public class SFXManager : MonoBehaviour
{
    private static SFXManager _instance;

    public static SFXManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SFXManager>();

                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("SFXManager");
                    _instance = singletonObject.AddComponent<SFXManager>();
                }
            }
            return _instance;
        }
        private set => _instance = value;
    }

    [SerializeField] private SoundLibrary soundLibrary;
    [SerializeField] private AudioSource sfxSource;

    private const string SFX_VOLUME_KEY = "SFXVolume";

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureComponents();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
        SetVolume(savedVolume);
    }

    private void EnsureComponents()
    {
        if (sfxSource == null)
        {
            sfxSource = GetComponent<AudioSource>();
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
            }
        }
        sfxSource.spatialBlend = 0f; // 2D Sound

        if (soundLibrary == null)
        {
            soundLibrary = GetComponent<SoundLibrary>();
        }
    }

    public void PlaySFX(string soundName)
    {
        EnsureComponents();

        if (soundLibrary == null)
        {
            Debug.LogError("❌ SoundLibrary fehlt auf dem SFXManager!");
            return;
        }

        AudioClip clip = soundLibrary.GetClipFromName(soundName);

        if (clip != null)
        {
            // PlayOneShot erlaubt es, mehrere Sounds gleichzeitig abzuspielen!
            sfxSource.PlayOneShot(clip);
        }
    }

    // --- LAUTSTÄRKE REGELUNG ---

    public void SetVolume(float volume)
    {
        float clampedVolume = Mathf.Clamp01(volume);

        if (sfxSource != null)
        {
            sfxSource.volume = clampedVolume;
        }

        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, clampedVolume);
        PlayerPrefs.Save();
    }

    public float GetVolume()
    {
        return PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
    }

    // Spielt einen Sound mit zufälliger Tonhöhe (Pitch) ab
public void PlaySFXWithPitch(string soundName, float minPitch = 0.9f, float maxPitch = 1.1f)
{
    EnsureComponents();

    if (soundLibrary == null) return;

    AudioClip clip = soundLibrary.GetClipFromName(soundName);

    if (clip != null)
    {
        // Setzt eine zufällige Tonhöhe für diesen einen Sound-Aufruf
        float randomPitch = Random.Range(minPitch, maxPitch);
        sfxSource.pitch = randomPitch;
        
        sfxSource.PlayOneShot(clip);

        // Nach dem Abspielen stellen wir den Pitch sicherheitshalber wieder auf normal (1.0)
        sfxSource.pitch = 1f; 
    }
}
}