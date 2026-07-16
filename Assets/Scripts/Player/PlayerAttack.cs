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

    private bool canAttack = true;
    private bool isAiming = false;
    private bool isCombatDisabled = false; // Steuert, ob der Spieler generell angreifen darf

    // Temporäre Schadensmodifikatoren
    private int originalLightAttackDamage;

    void Awake()
    {
        // Wir merken uns den Standard-Schaden für den Fall eines Boost-Resets
        originalLightAttackDamage = lightAttackDamage;
    }

    // --- NEU/PROPERTIES FÜR ANDERE SKRIPTE ---
    
    /// <summary>
    /// Erhöht den Schaden für eine bestimmte Dauer.
    /// Wird von InventorySlotUI aufgerufen (z.B. beim Trinken eines Stärketranks).
    /// </summary>
    /// <param name="boostAmount">Wie viel Schaden addiert wird.</param>
    /// <param name="duration">Wie lange der Boost hält (in Sekunden).</param>
    public void BoostDamage(int boostAmount, float duration)
    {
        StartCoroutine(DamageBoostCoroutine(boostAmount, duration));
    }

    private IEnumerator DamageBoostCoroutine(int boostAmount, float duration)
    {
        lightAttackDamage = originalLightAttackDamage + boostAmount;
        Debug.Log($"[Damage Boost] Schaden um {boostAmount} erhöht! Neuer Schaden: {lightAttackDamage} für {duration} Sekunden.");

        yield return new WaitForSeconds(duration);

        lightAttackDamage = originalLightAttackDamage;
        Debug.Log($"[Damage Boost] Vorbei! Schaden wieder normal: {lightAttackDamage}");
    }

    /// <summary>
    /// Gibt zurück, ob der Spieler gerade zielt. 
    /// Wird von PlayerMovement und PlayerCrosshairUI abgefragt.
    /// </summary>
    public bool IsAiming()
    {
        return isAiming;
    }

    /// <summary>
    /// Deaktiviert den Kampf (wird z.B. vom Inventar aufgerufen).
    /// </summary>
    public void DisableCombat()
    {
        isCombatDisabled = true;
        ResetCameras(); // Falls er beim Öffnen des Inventars gezielt hat, setzen wir das zurück
    }

    /// <summary>
    /// Aktiviert den Kampf wieder (wird beim Schließen des Inventars aufgerufen).
    /// </summary>
    public void EnableCombat()
    {
        isCombatDisabled = false;
    }

    // -----------------------------------------

    void Update()
    {
        // Wenn der Kampf komplett deaktiviert ist (z.B. Inventar offen), blockieren wir jeglichen Input
        if (isCombatDisabled) return;

        HandleInput();
    }

    void HandleInput()
    {
        // Rechtsklick halten zum Zielen (für Trankwurf)
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

        // LICHTER ANGRIFF: Reagiert jetzt auf Linksklick ODER Taste R!
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

        // Trank werfen über das separate PotionThrower-Skript
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

        // Trigger die Angriffs-Animation im Animator
        if (animator != null)
        {
            animator.SetTrigger("LightAttack");
        }

        // Schaden austeilen
        DealMeleeDamage(lightAttackRange, lightAttackDamage);

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    /// <summary>
    /// Sucht im Angriffsradius nach Gegnern und fügt ihnen Schaden zu.
    /// </summary>
    void DealMeleeDamage(float range, int damage)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, range);
        
        foreach (Collider hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();

            if (enemy == null)
            {
                enemy = hit.GetComponentInParent<EnemyHealth>();
            }

            if (enemy == null)
            {
                enemy = hit.GetComponentInChildren<EnemyHealth>();
            }

            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log($"[Nahkampf] {hit.name} erfolgreich getroffen! {damage} Schaden verursacht.");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, lightAttackRange);
    }
}