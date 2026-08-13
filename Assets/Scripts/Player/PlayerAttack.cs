using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Angriffs-Einstellungen")]
    [SerializeField] private int lightAttackDamage = 20;
    [SerializeField] private float lightAttackRange = 2f;
    [SerializeField] private float attackCooldown = 0.5f;
    [Tooltip("Verzögerung in Sekunden, bis Schaden und VFX nach Animationsstart auslösen")]
    [SerializeField] private float attackHitDelay = 0.15f; 

    [Header("Zielen & Kameras (Cinemachine)")]
    [SerializeField] private GameObject aimCamera;
    [SerializeField] private GameObject thirdPersonCamera;
    [SerializeField] private GameObject crosshairUI;

    [Header("Visuelle Effekte")]
    [Tooltip("Ziehe hier das Partikelsystem für den Schadensboost rein")]
    [SerializeField] private ParticleSystem damageBoostParticles;
    
    [Header("Angriffs-VFX (Slash / Kratzer)")]
    [Tooltip("Das Partikel-/VFX-Prefab für den Kratz- / Schwung-Effekt")]
    [SerializeField] private GameObject slashVFXPrefab;
    [Tooltip("Optional: Transform vor dem Spieler, wo der Effekt erscheinen soll")]
    [SerializeField] private Transform slashSpawnPoint;

    [Header("Audio (Angriff)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip attackSwingSound; 
    [SerializeField] private AudioClip attackHitSound;   

    private PlayerAnimationController animController;
    private bool canAttack = true;
    private bool isAiming = false;
    private bool isCombatDisabled = false;

    private int originalLightAttackDamage;
    private Coroutine currentBoostCoroutine;

    void Awake()
    {
        originalLightAttackDamage = lightAttackDamage;
        
        // Findet das Animation-Script zuverlässig
        animController = GetComponent<PlayerAnimationController>();
        if (animController == null)
        {
            animController = GetComponentInChildren<PlayerAnimationController>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (damageBoostParticles != null)
        {
            damageBoostParticles.Stop();
        }
    }

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
        
        if (animController != null) animController.SetAiming(true);
    }

    void ThrowPotion()
    {
        if (animController != null)
        {
            animController.SetAiming(false);
            animController.TriggerThrow();
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

        if (animController != null) animController.SetAiming(false);
    }

    IEnumerator PerformLightAttack()
    {
        canAttack = false;

        // 1. Animation starten
        if (animController != null)
        {
            animController.TriggerLightAttack();
        }

        // 2. Schwung-Sound direkt abspielen
        if (audioSource != null && attackSwingSound != null)
        {
            audioSource.PlayOneShot(attackSwingSound);
        }

        // 3. Kurz warten, bis der Schlag im Schwung ist
        if (attackHitDelay > 0f)
        {
            yield return new WaitForSeconds(attackHitDelay);
        }

        // 4. VFX erzeugen & Schaden berechnen
        SpawnSlashVFX();
        DealMeleeDamage(lightAttackRange, lightAttackDamage);

        // 5. Verbleibenden Cooldown abwarten
        float remainingCooldown = Mathf.Max(0f, attackCooldown - attackHitDelay);
        yield return new WaitForSeconds(remainingCooldown);

        canAttack = true;
    }

    private void SpawnSlashVFX()
    {
        if (slashVFXPrefab == null) return;

        Vector3 spawnPosition = slashSpawnPoint != null 
            ? slashSpawnPoint.position 
            : transform.position + transform.forward * 1.2f + Vector3.up * 1.0f;

        Quaternion spawnRotation = slashSpawnPoint != null 
            ? slashSpawnPoint.rotation 
            : transform.rotation;

        GameObject vfxInstance = Instantiate(slashVFXPrefab, spawnPosition, spawnRotation);
        Destroy(vfxInstance, 1.5f);
    }

    void DealMeleeDamage(float range, int damage)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, range);
        bool hitSomething = false;
        
        foreach (Collider hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

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

            DestroyableTarget destroyable = hit.GetComponent<DestroyableTarget>();
            if (destroyable == null) destroyable = hit.GetComponentInParent<DestroyableTarget>();

            if (destroyable != null)
            {
                destroyable.TakeDamage(damage);
                hitSomething = true;
                Debug.Log($"[Nahkampf] Zielscheibe {hit.name} getroffen!");
                continue;
            }

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