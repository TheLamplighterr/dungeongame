using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossUI : MonoBehaviour
{
    public static BossUI Instance { get; private set; }

    [Header("UI-Komponenten")]
    [SerializeField] private GameObject bossUIContainer; 
    [SerializeField] private Slider bossHealthSlider;

    [Header("Musik-Einstellungen")]
    [Tooltip("Exakter Name des Boss-Tracks in der MusicLibrary")]
    [SerializeField] private string bossMusicTrackName = "BossTheme";
    [Tooltip("Exakter Name des Sieg-Jingles in der MusicLibrary")]
    [SerializeField] private string victoryMusicTrackName = "VictoryJingle";
    [Tooltip("Exakter Name der normalen Dungeon-Musik in der MusicLibrary")]
    [SerializeField] private string normalMusicTrackName = "DungeonTheme";
    
    [Header("Zeiten")]
    [Tooltip("Verzögerung in Sekunden, bevor der Sieges-Jingle nach dem Tod startet")]
    [SerializeField] private float victoryDelay = 2.0f;
    [Tooltip("Dauer des Sieges-Jingles in Sekunden")]
    [SerializeField] private float victoryMusicDuration = 4.0f;

    private EnemyHealth activeBoss;
    private bool isFightActive = false;
    private bool isDefeated = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (bossUIContainer != null)
        {
            bossUIContainer.SetActive(false);
        }
    }

    public void StartBossFight(EnemyHealth boss = null)
    {
        if (isDefeated) return;

        if (boss != null && boss != activeBoss)
        {
            if (activeBoss != null) activeBoss.OnHit -= UpdateHealthSlider;
            activeBoss = boss;
            activeBoss.OnHit += UpdateHealthSlider;
        }

        isFightActive = true;

        if (bossUIContainer != null)
        {
            bossUIContainer.SetActive(true);
        }

        UpdateHealthSlider();

        // Boss-Musik starten
        if (MusicManager.Instance != null && !string.IsNullOrEmpty(bossMusicTrackName))
        {
            MusicManager.Instance.PlayMusic(bossMusicTrackName, 1.0f);
        }
    }

    private void UpdateHealthSlider()
    {
        if (activeBoss == null || bossHealthSlider == null) return;

        float healthPercentage = (float)activeBoss.currentHealth / (float)activeBoss.maxHealth;
        bossHealthSlider.value = Mathf.Clamp01(healthPercentage);

        // Sobald KP <= 0 sind: Event SOFORT deabonnieren und nur 1x die Coroutine starten!
        if (activeBoss.currentHealth <= 0 && !isDefeated)
        {
            isDefeated = true; // Sofort blockieren
            activeBoss.OnHit -= UpdateHealthSlider; // Event abmelden, damit kein 2. Hit reinkommen kann!
            StartCoroutine(VictorySequence());
        }
    }

    private IEnumerator VictorySequence()
    {
        isFightActive = false;

        // 1. Warten (Verzögerung nach dem Tod, z. B. 2 Sekunden)
        yield return new WaitForSeconds(victoryDelay);

        Debug.Log("[BOSS-UI] Boss besiegt! Spiele Victory-Jingle...");

        // 2. Victory Jingle genau einmal starten
        if (MusicManager.Instance != null && !string.IsNullOrEmpty(victoryMusicTrackName))
        {
            MusicManager.Instance.PlayMusic(victoryMusicTrackName, 0.2f);
        }

        // 3. Warten bis Jingle zu Ende gespielt wurde
        yield return new WaitForSeconds(victoryMusicDuration);

        // 4. UI ausblenden
        if (bossUIContainer != null)
        {
            bossUIContainer.SetActive(false);
        }

        // 5. Zurück zur Dungeon-Musik
        if (MusicManager.Instance != null && !string.IsNullOrEmpty(normalMusicTrackName))
        {
            MusicManager.Instance.PlayMusic(normalMusicTrackName, 1.5f);
        }
    }

    public void CancelBossFight()
    {
        if (!isFightActive || isDefeated) return;

        isFightActive = false;

        if (bossUIContainer != null)
        {
            bossUIContainer.SetActive(false);
        }

        // Zurück zur Dungeon-Musik
        if (MusicManager.Instance != null && !string.IsNullOrEmpty(normalMusicTrackName))
        {
            MusicManager.Instance.PlayMusic(normalMusicTrackName, 1.0f);
        }
    }

    private void OnDestroy()
    {
        if (activeBoss != null)
        {
            activeBoss.OnHit -= UpdateHealthSlider;
        }
    }
}