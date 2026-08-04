using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private static MusicManager _instance;

    public static MusicManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<MusicManager>();

                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("MusicManager (Auto-Created)");
                    _instance = singletonObject.AddComponent<MusicManager>();
                }
            }
            return _instance;
        }
        private set
        {
            _instance = value;
        }
    }

    [SerializeField] private MusicLibrary musicLibrary;
    [SerializeField] private AudioSource musicSource;

    private Coroutine crossfadeCoroutine;
    private const string VOLUME_KEY = "MusicVolume";

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
        // Lädt die gespeicherte Lautstärke (Standard: 1.0)
        float savedVolume = PlayerPrefs.GetFloat(VOLUME_KEY, 1f);
        SetVolume(savedVolume);
    }

    private void EnsureComponents()
    {
        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
            }
        }
        musicSource.spatialBlend = 0f; // 2D Sound
        musicSource.loop = true;

        if (musicLibrary == null)
        {
            musicLibrary = GetComponent<MusicLibrary>();
        }
    }

    public void PlayMusic(string trackName, float fadeDuration = 0.5f)
    {
        EnsureComponents();

        if (musicLibrary == null)
        {
            Debug.LogError("❌ MusicLibrary fehlt auf dem MusicManager!");
            return;
        }

        AudioClip nextTrack = musicLibrary.GetClipFromName(trackName);

        if (nextTrack == null)
        {
            Debug.LogError($"❌ Track '{trackName}' wurde in der MusicLibrary nicht gefunden!");
            return;
        }

        if (musicSource.clip == nextTrack && musicSource.isPlaying) return;

        if (crossfadeCoroutine != null) StopCoroutine(crossfadeCoroutine);
        crossfadeCoroutine = StartCoroutine(AnimateMusicCrossfade(nextTrack, fadeDuration));
    }

    private IEnumerator AnimateMusicCrossfade(AudioClip nextTrack, float fadeDuration)
    {
        float targetVolume = GetVolume(); // Nutzt die eingestellte Lautstärke als Maximum
        float startVolume = musicSource.volume;
        float percent = 0;

        if (musicSource.isPlaying)
        {
            while (percent < 1f)
            {
                percent += Time.deltaTime / fadeDuration;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, percent);
                yield return null;
            }
        }

        musicSource.clip = nextTrack;
        musicSource.Play();

        percent = 0;
        while (percent < 1f)
        {
            percent += Time.deltaTime / fadeDuration;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, percent);
            yield return null;
        }

        musicSource.volume = targetVolume;
    }

    // --- LAUTSTÄRKE REGELUNG ---

    public void SetVolume(float volume)
    {
        float clampedVolume = Mathf.Clamp01(volume);

        if (musicSource != null)
        {
            musicSource.volume = clampedVolume;
        }

        PlayerPrefs.SetFloat(VOLUME_KEY, clampedVolume);
        PlayerPrefs.Save();
    }

    public float GetVolume()
    {
        return PlayerPrefs.GetFloat(VOLUME_KEY, 1f);
    }
}