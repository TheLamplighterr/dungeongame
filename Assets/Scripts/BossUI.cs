using UnityEngine;
using UnityEngine.UI;

public class BossUI : MonoBehaviour
{
    public static BossUI Instance { get; private set; }

    [Header("UI-Komponenten")]
    [SerializeField] private GameObject bossUIContainer; 
    [SerializeField] private Slider bossHealthSlider;

    private EnemyHealth activeBoss;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[BOSS-UI] Singleton erfolgreich initialisiert.");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Zu Beginn unsichtbar machen
        if (bossUIContainer != null)
        {
            bossUIContainer.SetActive(false);
            Debug.Log("[BOSS-UI] Container beim Start ausgeblendet.");
        }
        else
        {
            Debug.LogError("[BOSS-UI] FEHLER: Kein 'bossUIContainer' im Inspector zugewiesen!");
        }
    }

    public void StartBossFight(EnemyHealth boss)
    {
        activeBoss = boss;
        
        if (bossUIContainer != null)
        {
            bossUIContainer.SetActive(true);
            Debug.Log($"[BOSS-UI] Container wurde für {boss.gameObject.name} AKTIVIERT!");
        }
        else
        {
            Debug.LogError("[BOSS-UI] Aktivierung fehlgeschlagen: Kein Container zugewiesen!");
        }

        if (activeBoss != null)
        {
            activeBoss.OnHit += UpdateBossUI;
            
            // WICHTIG: Sofort befüllen, damit der Balken von Sekunde 1 an voll (100%) angezeigt wird!
            if (bossHealthSlider != null)
            {
                bossHealthSlider.minValue = 0f;
                bossHealthSlider.maxValue = 1f;
                
                // Berechne den aktuellen Prozentsatz (sollte am Start 1.0f sein)
                float healthPercentage = (float)activeBoss.currentHealth / (float)activeBoss.maxHealth;
                bossHealthSlider.value = healthPercentage;
            }
        }
    }

    private void UpdateBossUI()
    {
        if (activeBoss == null || bossHealthSlider == null) return;

        // Präzise Prozentberechnung
        float healthPercentage = (float)activeBoss.currentHealth / (float)activeBoss.maxHealth;
        bossHealthSlider.value = Mathf.Clamp01(healthPercentage);

        Debug.Log($"[BOSS-UI] Slider aktualisiert auf: {bossHealthSlider.value * 100f}%");

        if (activeBoss.currentHealth <= 0)
        {
            Invoke(nameof(EndBossFight), 2f);
        }
    }
    private void EndBossFight()
    {
        if (activeBoss != null)
        {
            activeBoss.OnHit -= UpdateBossUI;
        }
        
        activeBoss = null;

        if (bossUIContainer != null)
        {
            bossUIContainer.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (activeBoss != null)
        {
            activeBoss.OnHit -= UpdateBossUI;
        }
    }
}