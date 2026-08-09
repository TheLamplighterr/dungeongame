using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [Tooltip("Das maximale Leben des Gegners.")]
    public int maxHealth = 100;
    
    // Wir initialisieren currentHealth im Code, damit es beim Start immer voll ist.
    public int currentHealth { get; private set; }

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

    [Header("Hit VFX")]
    public GameObject hitEffectPrefab;
    public float hitVfxScale = 1.5f;

    [Header("Death VFX")]
    public GameObject deathEffectPrefab;
    public float deathVfxScale = 2f;

    public event Action OnHit;

    private Color originalColor;
    private bool isDead;

    [Header("Death Animation")]
    public Animator animator;
    public string deathAnimation = "";
    public float deathAnimationLength = 2f;

    void Start()
    {
        // Sicherheitshalber fangen wir falsche Inspector-Eingaben ab
        if (maxHealth <= 0) maxHealth = 100;

        // Gegner startet IMMER mit vollem Leben
        currentHealth = maxHealth;

        // Wir prüfen, wie der zugewiesene Slider im Editor konfiguriert ist.
        // Wenn er auf maxValue = 100 (oder höher) steht, lassen wir ihn so.
        // Wenn er auf maxValue = 1 steht, nutzen wir das Prozent-System.
        if (healthSlider != null)
        {
            // Wenn der Slider im Editor unberührt ist, passen wir sein Maximum an seine HP an
            if (healthSlider.maxValue > 1f)
            {
                healthSlider.minValue = 0f;
                healthSlider.maxValue = maxHealth;
            }
        }

        if (fillImage != null)
            fillImage.color = Color.red;

        if (healthCanvas != null)
            healthCanvas.enabled = false;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }

        if (enemyRenderer != null)
            originalColor = enemyRenderer.material.color;

        // Direkt die UI befüllen, damit der Balken bei 100% startet
        UpdateHealthUI();
    }

    void LateUpdate()
    {
        if (isDead) return;

        if (fillImage != null)
            fillImage.color = Color.red;

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

        Debug.Log($"<color=yellow>[DAMAGE]</color> {gameObject.name} hat {damage} Schaden erlitten. HP: {currentHealth}/{maxHealth}");

        UpdateHealthUI();

        if (healthCanvas != null)
            healthCanvas.enabled = true;

        SpawnHitEffect();

        // Das Event feuern, damit die Boss-UI (falls aktiv) benachrichtigt wird
        OnHit?.Invoke();

        if (enemyRenderer != null)
            StartCoroutine(FlashRed());

        if (currentHealth <= 0)
            Die();
    }

    public void UpdateHealthUI()
    {
        if (healthSlider == null) return;

        // INTELLIGENTES UI-UPDATE:
        // Wir prüfen, ob der Slider im Editor auf das Prozent-System (Maximum = 1) eingestellt ist.
        if (healthSlider.maxValue <= 1.05f) // Ein kleiner Puffer für Floats (z.B. 1.0f)
        {
            float healthPercentage = (float)currentHealth / (float)maxHealth;
            healthSlider.value = Mathf.Clamp01(healthPercentage);
        }
        else
        {
            // Klassischer Slider (z.B. 0 bis 100 oder 200)
            healthSlider.maxValue = maxHealth; // Zwingend synchronisieren
            healthSlider.value = currentHealth;
        }
    }

    void SpawnHitEffect()
    {
        if (hitEffectPrefab == null)
            return;

        Vector3 spawnPos = transform.position + Vector3.up * 0.3f;
        GameObject fx = Instantiate(hitEffectPrefab, spawnPos, Quaternion.identity);
        fx.transform.localScale = Vector3.one * hitVfxScale;
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

    if (RunStatsManager.Instance != null)
        RunStatsManager.Instance.AddKill();

    SpawnDeathEffect();

    // 1. NEU: Benachrichtige die PoisonPlant-Komponente (falls vorhanden)
    if (TryGetComponent(out PoisonPlant plant))
    {
        plant.PlayDeathAnimation();
    }

    // Physikalische Komponenten ausschalten
    if (TryGetComponent(out Collider col))
        col.enabled = false;

    if (TryGetComponent(out Rigidbody rb))
        rb.isKinematic = true;

    if (TryGetComponent(out UnityEngine.AI.NavMeshAgent agent))
        agent.isStopped = true;

    MonoBehaviour ai = GetComponent<BaseEnemyAI>();
    if (ai != null)
        ai.enabled = false;

    // 2. Animiere & Zerstöre das Objekt
    // Wenn PoisonPlant die Todesanimation verwaltet, übernimmt diese das Abspielen
    if (animator != null && !string.IsNullOrEmpty(deathAnimation))
    {
        animator.CrossFade(deathAnimation, 0.1f);
        Destroy(gameObject, deathAnimationLength);
    }
    else
    {
        // Falls keine extra Todesanimation im Health-Skript definiert ist,
        // geben wir der Pflanze etwas Zeit für ihre eigene Animation vor dem Destroy
        Destroy(gameObject, deathAnimationLength > 0 ? deathAnimationLength : 3f);
    }
}
    void SpawnDeathEffect()
    {
        if (deathEffectPrefab == null)
            return;

        Vector3 pos = transform.position + Vector3.up * 0.5f;
        GameObject fx = Instantiate(deathEffectPrefab, pos, Quaternion.identity);
        fx.transform.localScale = Vector3.one * deathVfxScale;
    }
}