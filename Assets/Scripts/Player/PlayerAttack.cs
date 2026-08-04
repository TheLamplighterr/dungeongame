using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Angriffs-Einstellungen")]
    [SerializeField] private int lightAttackDamage = 20;
    [SerializeField] private float lightAttackRange = 2f;
    [SerializeField] private float attackCooldown = 0.5f;

    [Header("Zielen & Kameras (Cinemachine)")]
    [SerializeField] private GameObject aimCamera;
    [SerializeField] private GameObject thirdPersonCamera;
    [SerializeField] private GameObject crosshairUI;

    [Header("Animationen")]
    [SerializeField] private Animator animator;

    [Header("Visuelle Effekte")]
    [Tooltip("Ziehe hier das Partikelsystem für den Schadensboost rein")]
    [SerializeField] private ParticleSystem damageBoostParticles;

    [Header("Audio (Angriff)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip attackSwingSound; // Slash / Kratz-Geräusch
    [SerializeField] private AudioClip attackHitSound;   // Fleisch / Treffer-Geräusch

    private bool canAttack = true;
    private bool isAiming = false;
    private bool isCombatDisabled = false;

    private int originalLightAttackDamage;
    private Coroutine currentBoostCoroutine;

    void Awake()
    {
        originalLightAttackDamage = lightAttackDamage;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (damageBoostParticles != null)
        {
            damageBoostParticles.Stop();
        }
    }

    // =========================================================
    // TEMPORÄRER BOOST (Trank / Potion)
    // =========================================================
    public void BoostDamage(int boostAmount, float duration)
    {
        if (currentBoostCoroutine != null)
        {
            StopCoroutine(currentBoostCoroutine);
        }

        currentBoostCoroutine = StartCoroutine(DamageBoostCoroutine(boostAmount, duration));
    }

    private IEnumerator DamageBoostCoroutine(int boostAmount, float duration)
    {
        // Addiert den Boost auf den AKTUELLEN (evt. bereits permanent erhöhten) Schaden
        int previousDamage = lightAttackDamage;
        lightAttackDamage += boostAmount;
        
        Debug.Log($"[Damage Boost] Temp-Schaden um {boostAmount} erhöht! Neuer Schaden: {lightAttackDamage} für {duration}s.");

        if (damageBoostParticles != null)
        {
            damageBoostParticles.Play();
        }

        yield return new WaitForSeconds(duration);

        if (damageBoostParticles != null)
        {
            damageBoostParticles.Stop();
        }

        // Setzt exakt auf den Wert vor dem temporären Boost zurück
        lightAttackDamage = previousDamage;
        Debug.Log($"[Damage Boost] Vorbei! Schaden wieder auf: {lightAttackDamage}");
        currentBoostCoroutine = null;
    }

    // =========================================================
    // PERMANENTER BOOST (Rüstung / Amulett / Ausrüstung) - NEU!
    // =========================================================
    public void AddPermanentDamage(int boostAmount)
    {
        originalLightAttackDamage += boostAmount;
        lightAttackDamage += boostAmount;

        if (damageBoostParticles != null)
        {
            damageBoostParticles.Play(); // Zeigt kurz Partikel als Feedback
        }

        Debug.Log($"[Dauerhafter Boost] Schaden dauerhaft um {boostAmount} erhöht! Neugrundschaden: {lightAttackDamage}");
    }

    public bool IsAiming() => isAiming;

    public void DisableCombat()
    {
        isCombatDisabled = true;
        ResetCameras();
    }

    public void EnableCombat()
    {
        isCombatDisabled = false;
    }

    void Update()
    {
        if (isCombatDisabled) return;
        HandleInput();
    }

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            StartAiming();
        }
        if (Input.GetMouseButtonUp(1))
        {
            if (isAiming)
            {
                ThrowPotion();
            }
        }

        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.R)) && canAttack && !isAiming)
        {
            StartCoroutine(PerformLightAttack());
        }
    }

    void StartAiming()
    {
        isAiming = true;
        if (aimCamera != null) aimCamera.SetActive(true);
        if (thirdPersonCamera != null) thirdPersonCamera.SetActive(false);
        if (crosshairUI != null) crosshairUI.SetActive(true);
        if (animator != null) animator.SetBool("Aim", true);
    }

    void ThrowPotion()
    {
        if (animator != null)
        {
            animator.SetBool("Aim", false);
            animator.SetTrigger("Throw");
        }

        PotionThrower thrower = GetComponent<PotionThrower>();
        if (thrower != null)
        {
            thrower.Throw();
        }
        else
        {
            Debug.LogWarning("PlayerAttack: PotionThrower-Komponente auf dem Spieler nicht gefunden!");
        }

        ResetCameras();
    }

    void ResetCameras()
    {
        isAiming = false;
        if (aimCamera != null) aimCamera.SetActive(false);
        if (thirdPersonCamera != null) thirdPersonCamera.SetActive(true);
        if (crosshairUI != null) crosshairUI.SetActive(false);
    }

    IEnumerator PerformLightAttack()
    {
        canAttack = false;

        if (animator != null)
        {
            animator.SetTrigger("LightAttack");
        }

        if (audioSource != null && attackSwingSound != null)
        {
            audioSource.PlayOneShot(attackSwingSound);
        }

        DealMeleeDamage(lightAttackRange, lightAttackDamage);

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    void DealMeleeDamage(float range, int damage)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, range);
        bool hitEnemy = false;
        
        foreach (Collider hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy == null) enemy = hit.GetComponentInParent<EnemyHealth>();
            if (enemy == null) enemy = hit.GetComponentInChildren<EnemyHealth>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                hitEnemy = true;
                Debug.Log($"[Nahkampf] {hit.name} erfolgreich getroffen! {damage} Schaden verursacht.");
            }
        }

        if (hitEnemy && audioSource != null && attackHitSound != null)
        {
            audioSource.PlayOneShot(attackHitSound);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, lightAttackRange);
    }
}