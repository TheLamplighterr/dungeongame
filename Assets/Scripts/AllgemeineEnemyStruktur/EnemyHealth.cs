using System;
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
        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
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

        UpdateHealthUI();

        if (healthCanvas != null)
            healthCanvas.enabled = true;

        SpawnHitEffect();

        OnHit?.Invoke();

        if (enemyRenderer != null)
            StartCoroutine(FlashRed());

        if (currentHealth <= 0)
            Die();
    }

    void UpdateHealthUI()
    {
        if (healthSlider != null)
            healthSlider.value = currentHealth;

        if (fillImage != null)
            fillImage.color = Color.red;
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

    RunStatsManager.Instance.AddKill();

    SpawnDeathEffect();

    if (TryGetComponent(out Collider col))
        col.enabled = false;

    if (TryGetComponent(out Rigidbody rb))
        rb.isKinematic = true;

    if (TryGetComponent(out UnityEngine.AI.NavMeshAgent agent))
        agent.isStopped = true;

    // KI deaktivieren
    MonoBehaviour ai = GetComponent<BaseEnemyAI>();
    if (ai != null)
        ai.enabled = false;

    if (animator != null && !string.IsNullOrEmpty(deathAnimation))
    {
        animator.CrossFade(deathAnimation, 0.1f);

        // Renderer NICHT sofort ausschalten!
        Destroy(gameObject, deathAnimationLength);
    }
    else
    {
        // Gegner ohne Todesanimation
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;

        Destroy(gameObject, 2f);
    }
}

    void SpawnDeathEffect()
    {
        if (deathEffectPrefab == null)
            return;

        Vector3 pos = transform.position + Vector3.up * 0.5f;

        GameObject fx = Instantiate(deathEffectPrefab, pos, Quaternion.identity);
        fx.transform.localScale = Vector3.one * deathVfxScale;

        Debug.Log(" Death VFX spawned");
    }
}