using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Knockback Settings")]
    [Tooltip("Standard-Stärke des Knockbacks bei Treffern")]
    public float defaultKnockbackForce = 8f;

    [Header("UI")]
    public Slider healthSlider;
    public Image fillImage;

    [Header("Damage Flash")]
    public Image damageFlash;
    public float flashSpeed = 5f;
    private float flashAlpha;

    [Header("Debug")]
    public bool debugLogs = true;

    private bool isDead = false;
    private PlayerMovement playerMovement;

    [Header("Game Over")]
    public GameObject gameOverPanel;
    public GameOverUI gameOverUI;

    [Header("Heal VFX")]
    public ParticleSystem healVFX;

    void Start()
    {
        currentHealth = maxHealth;
        playerMovement = GetComponent<PlayerMovement>();

        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = maxHealth;
        }

        UpdateUI();
    }

    void Update()
    {
        if (damageFlash != null)
        {
            flashAlpha = Mathf.Lerp(flashAlpha, 0f, Time.deltaTime * flashSpeed);
            damageFlash.color = new Color(1f, 0f, 0f, flashAlpha);
        }
    }

    // Abwärtskompatible Methode (falls im Code nur TakeDamage(10) aufgerufen wird)
    public void TakeDamage(int damage)
    {
        TakeDamage(damage, Vector3.zero, 0f);
    }

    // Überladene Methode mit Knockback-Richtung & Stärke
    public void TakeDamage(int damage, Vector3 attackerPosition, float knockbackForce = 0f)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (debugLogs)
            Debug.Log($"[PlayerHealth] Took {damage} damage → HP: {currentHealth}/{maxHealth}");

        flashAlpha = 1f;

        // Knockback berechnen & anwenden
        if (playerMovement != null && attackerPosition != Vector3.zero)
        {
            Vector3 knockbackDir = transform.position - attackerPosition;
            float force = knockbackForce > 0f ? knockbackForce : defaultKnockbackForce;
            playerMovement.ApplyKnockback(knockbackDir, force);
        }

        UpdateUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"❤️ Healed +{amount} → HP: {currentHealth}/{maxHealth}");

        UpdateUI();

        if (healVFX != null)
        {
            healVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            healVFX.Play();
        }
    }

    void UpdateUI()
    {
        if (healthSlider == null) return;

        healthSlider.value = currentHealth;

        if (fillImage != null)
        {
            float t = (float)currentHealth / maxHealth;
            fillImage.color = Color.Lerp(Color.red, Color.green, t);
        }
    }

   void Die()
    {
        if (isDead) return;

        isDead = true;

        Debug.Log("PLAYER DIED");

        // 1. Run-Statistiken beenden & als Highscore speichern!
        if (RunStatsManager.Instance != null)
        {
            RunStatsManager.Instance.EndRun();
        }
        else
        {
            Debug.LogWarning("⚠️ RunStatsManager.Instance ist NULL beim Sterben!");
        }

        // 2. Game Over UI anzeigen
        if (gameOverUI != null)
        {
            gameOverUI.ShowGameOver();
        }

        Time.timeScale = 0f;
    }
}