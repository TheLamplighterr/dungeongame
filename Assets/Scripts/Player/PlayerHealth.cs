using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

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

    [Header("Game Over")]
    public GameObject gameOverPanel;
    public GameOverUI gameOverUI;

    [Header("Heal VFX")]
    public ParticleSystem healVFX;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = maxHealth;
        }

        UpdateUI();
    }

    void Update()
    {
        // Damage Flash Fade Out
        if (damageFlash != null)
        {
            flashAlpha = Mathf.Lerp(flashAlpha, 0f, Time.deltaTime * flashSpeed);
            damageFlash.color = new Color(1f, 0f, 0f, flashAlpha);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (debugLogs)
            Debug.Log($"[PlayerHealth] Took {damage} damage → HP: {currentHealth}/{maxHealth}");

        flashAlpha = 1f;

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

    // =========================
    // HEAL VFX (robust)
    // =========================
    if (healVFX != null)
    {
        healVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        healVFX.Play();
    }
}

    IEnumerator StopHealVFX()
    {
        yield return new WaitForSeconds(1f);

        if (healVFX != null)
            healVFX.Stop();
    }

    void UpdateUI()
{
    Debug.Log($"[UI] UpdateUI called | HP: {currentHealth}");

    if (healthSlider == null)
    {
        Debug.LogError("❌ healthSlider ist NULL!");
        return;
    }

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

        Debug.Log(" PLAYER DIED");

        if (gameOverUI != null)
        {
            gameOverUI.ShowGameOver();
        }
        else
        {
            Debug.LogError("GameOverUI NICHT im Inspector gesetzt!");
        }

        Time.timeScale = 0f;
    }
}