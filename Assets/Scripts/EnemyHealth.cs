using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("UI")]
    public Slider healthSlider;
    public Canvas healthCanvas;
    public Image fillImage;

    [Header("Detection")]
    public Transform player;
    public float showRange = 6f;

    [Header("Hit Flash")]
    public Renderer enemyRenderer;
    public Color flashColor = Color.red;
    public float flashDuration = 0.1f;

    private Color originalColor;
    private bool isDead;

    void Start()
    {
        currentHealth = maxHealth;

        // Healthbar initialisieren
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        // Enemy Healthbar immer rot
        if (fillImage != null)
        {
            fillImage.color = Color.red;
        }

        // Healthbar zunächst ausblenden
        if (healthCanvas != null)
        {
            healthCanvas.enabled = false;
        }

        // Player automatisch finden
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");

            if (p != null)
                player = p.transform;
        }

        // Ursprüngliche Materialfarbe speichern
        if (enemyRenderer != null)
        {
            originalColor = enemyRenderer.material.color;
        }
    }

    void LateUpdate()
    {
        if (isDead) return;

        HandleVisibility();
        FaceCamera();
    }

    void HandleVisibility()
    {
        if (player == null || healthCanvas == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        healthCanvas.enabled = distance <= showRange;
    }

    void FaceCamera()
    {
        if (healthCanvas == null || Camera.main == null)
            return;

        healthCanvas.transform.forward = Camera.main.transform.forward;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI();

        // Healthbar zeigen sobald Schaden genommen wird
        if (healthCanvas != null)
            healthCanvas.enabled = true;

        // Treffer-Effekt
        StartCoroutine(FlashRed());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthUI()
    {
        if (healthSlider != null)
            healthSlider.value = currentHealth;

        // Sicherheitshalber immer rot halten
        if (fillImage != null)
            fillImage.color = Color.red;
    }

    IEnumerator FlashRed()
    {
        if (enemyRenderer == null)
            yield break;

        enemyRenderer.material.color = flashColor;

        yield return new WaitForSeconds(flashDuration);

        enemyRenderer.material.color = originalColor;
    }

    void Die()
    {
        isDead = true;

        Destroy(gameObject);
    }
}