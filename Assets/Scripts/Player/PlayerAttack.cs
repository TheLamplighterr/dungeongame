using UnityEngine;
using System.Collections;
using Unity.Cinemachine; // Wichtig für Cinemachine v3

public class PlayerAttack : MonoBehaviour
{
    [Header("Animator")]
    public Animator animator;

    [Header("Enemy")]
    public LayerMask enemyLayer;

    [Header("Light Attack")]
    public float lightAttackRange = 3f;
    public int lightDamage = 25;
    public float lightCooldown = 0.7f;

    [Header("Heavy Attack")]
    public float heavyAttackRange = 4f;
    public int heavyDamage = 50;
    public float heavyCooldown = 1.5f;

    [Header("VFX")]
    public ParticleSystem damageBoostVFX;

    [Header("Aim Camera System")]
    public CinemachineCamera normalCamera; 
    public CinemachineCamera aimCamera;   
    public float holdThreshold = 0.35f;    

    [Header("UI Elemente")]
    public GameObject crosshairUI; // ZIEHE HIER DEIN FADENKREUZ-IMAGE REIN!

    private bool canLightAttack = true;
    private bool canHeavyAttack = true;
    private bool controlsEnabled = true;

    private bool isHolding = false;
    private bool isAiming = false; 
    private float holdStartTime;

    private Coroutine attackBoostRoutine;
    private int baseLightDamage;
    private int baseHeavyDamage;

    public bool IsAiming()
    {
        return isAiming;
    }

    void Start()
    {
        baseLightDamage = lightDamage;
        baseHeavyDamage = heavyDamage;

        // Sicherheits-Check beim Start: Kameras zurücksetzen und Fadenkreuz aus
        ResetCameras();
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        if (!controlsEnabled)
            return;

        // 1. Linksklick drücken
        if (Input.GetMouseButtonDown(0))
        {
            holdStartTime = Time.time;
            isHolding = true;
        }

        // 2. Linksklick halten -> Aim-Modus aktivieren
        if (isHolding && Input.GetMouseButton(0))
        {
            if (!isAiming && (Time.time - holdStartTime >= holdThreshold))
            {
                EnterAimMode();
            }
        }

        // 3. Linksklick loslassen
        if (Input.GetMouseButtonUp(0))
        {
            if (isAiming)
            {
                ThrowPotion(); 
            }
            else if (canLightAttack)
            {
                StartCoroutine(LightAttack()); 
            }

            isHolding = false;
        }

        // 4. Heavy Attack (Rechtsklick)
        if (Input.GetMouseButtonDown(1))
        {
            if (canHeavyAttack && !isAiming) 
            {
                StartCoroutine(HeavyAttack());
            }
        }
    }

    //--------------------------------------------------
    // AIM MODUS (KAMERA-WECHSEL & FADENKREUZ)
    //--------------------------------------------------
    void EnterAimMode()
    {
        isAiming = true;
        Debug.Log("Aim Mode Aktiviert!");

        if (animator != null)
            animator.SetBool("Aim", true);

        // Kameras umschalten
        if (normalCamera != null) normalCamera.Priority = 0;
        if (aimCamera != null) aimCamera.Priority = 20;

        // Fadenkreuz anzeigen!
        if (crosshairUI != null)
            crosshairUI.SetActive(true);
    }

    void ThrowPotion()
    {
        Debug.Log("Trank geworfen!");

        if (animator != null)
        {
            animator.SetBool("Aim", false);
            animator.SetTrigger("Throw");
        }

        ResetCameras();
    }

    void ResetCameras()
    {
        isAiming = false;

        if (normalCamera != null) normalCamera.Priority = 20;
        if (aimCamera != null) aimCamera.Priority = 0;

        // Fadenkreuz verstecken!
        if (crosshairUI != null)
            crosshairUI.SetActive(false);
    }

    //--------------------------------------------------
    // NORMAL ATTACKS (COOLDOWNS)
    //--------------------------------------------------
    IEnumerator LightAttack()
    {
        canLightAttack = false;
        if (animator != null) animator.SetTrigger("LightAttack");
        yield return new WaitForSeconds(lightCooldown);
        canLightAttack = true;
    }

    IEnumerator HeavyAttack()
    {
        canHeavyAttack = false;
        if (animator != null) animator.SetTrigger("HeavyAttack");
        yield return new WaitForSeconds(heavyCooldown);
        canHeavyAttack = true;
    }

    public void LightAttackHit() { PerformAttack(lightAttackRange, lightDamage); }
    public void HeavyAttackHit() { PerformAttack(heavyAttackRange, heavyDamage); }

    void PerformAttack(float range, int damage)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, range);
        foreach (Collider hit in hits)
        {
            EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();
            if (enemy != null) enemy.TakeDamage(damage);
        }
    }

    public void BoostDamage(int bonus, float duration)
    {
        if (attackBoostRoutine != null) StopCoroutine(attackBoostRoutine);
        attackBoostRoutine = StartCoroutine(DamageBoostRoutine(bonus, duration));
    }

    IEnumerator DamageBoostRoutine(int bonus, float duration)
    {
        lightDamage = baseLightDamage + bonus; heavyDamage = baseHeavyDamage + bonus;
        if (damageBoostVFX != null) damageBoostVFX.Play();
        yield return new WaitForSeconds(duration);
        lightDamage = baseLightDamage; heavyDamage = baseHeavyDamage;
        if (damageBoostVFX != null) damageBoostVFX.Stop();
        attackBoostRoutine = null;
    }

    public void EnableCombat() { controlsEnabled = true; }
    public void DisableCombat() { ResetCameras(); controlsEnabled = false; }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, lightAttackRange);
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, heavyAttackRange);
    }
}