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

        lightAttackDamage = previousDamage;
        Debug.Log($"[Damage Boost] Vorbei! Schaden wieder auf: {lightAttackDamage}");
        currentBoostCoroutine = null;
    }

    // =========================================================
    // PERMANENTER BOOST (Rüstung / Amulett / Ausrüstung)
    // =========================================================
    public void AddPermanentDamage(int boostAmount)
    {
        originalLightAttackDamage += boostAmount;
        lightAttackDamage += boostAmount;

        if (damageBoostParticles != null)
        {
            damageBoostParticles.Play();
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

    // =========================================================
    // MELEE TREFFERLOGIK (Gegner, Zielscheiben & Schalter)
    // =========================================================
    void DealMeleeDamage(float range, int damage)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, range);
        bool hitSomething = false;
        
        foreach (Collider hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            // 1. Gegner prüfen
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy == null) enemy = hit.GetComponentInParent<EnemyHealth>();
            if (enemy == null) enemy = hit.GetComponentInChildren<EnemyHealth>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                hitSomething = true;
                Debug.Log($"[Nahkampf] Gegner {hit.name} getroffen! {damage} Schaden.");
                continue;
            }

            // 2. Zerstörbares Ziel prüfen
            DestroyableTarget destroyable = hit.GetComponent<DestroyableTarget>();
            if (destroyable == null) destroyable = hit.GetComponentInParent<DestroyableTarget>();

            if (destroyable != null)
            {
                destroyable.TakeDamage(damage);
                hitSomething = true;
                Debug.Log($"[Nahkampf] Zielscheibe {hit.name} getroffen!");
                continue;
            }

            // 3. Kristall-Schalter prüfen
            CrystalSwitch crystal = hit.GetComponent<CrystalSwitch>();
            if (crystal == null) crystal = hit.GetComponentInParent<CrystalSwitch>();

            if (crystal != null)
            {
                crystal.TakeDamage(damage);
                hitSomething = true;
                Debug.Log($"[Nahkampf] Kristall-Schalter {hit.name} aktiviert!");
            }
        }

        if (hitSomething && audioSource != null && attackHitSound != null)
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