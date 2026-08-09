using UnityEngine;
using System.Collections;

public class PoisonPlant : MonoBehaviour
{
    [Header("Gift-Eigenschaften")]
    public int poisonDamage = 5;

    [Tooltip("Intervall in Sekunden für den Gift-Schaden während des Loops")]
    public float damageInterval = 1.0f;

    [Header("Animation State Names")]
    public string idleAnimName = "Plant_Idle";
    public string attackStartAnimName = "Plant_AttackStart";
    public string attackLoopAnimName = "Plant_Attack";
    public string attackEndAnimName = "Plant_AttackEnd";
    public string deathAnimName = "Plant_Death";

    [Header("Animation Timing (in Sekunden)")]
    public float attackStartDuration = 1.167f;
    public float attackEndDuration = 1.024f;

    [Header("Animation & VFX Anchor")]
    public Animator animator;
    
    [Tooltip("Das Partikelsystem als PREFAB aus dem Project-Ordner!")]
    public GameObject poisonVFXPrefab;

    [Tooltip("Ein leeres GameObject an der Pflanze, wo die Wolke entstehen soll")]
    public Transform vfxSpawnPoint;

    private PlayerHealth playerHealth;
    private Coroutine attackSequenceCoroutine;
    private Coroutine exitSequenceCoroutine;
    private GameObject currentVFXInstance;
    private bool isDead = false;
    private string currentAnimation = "";

    void Start()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator != null)
            animator.applyRootMotion = false;

        PlayAnimation(idleAnimName);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        if (other.CompareTag("Player"))
        {
            playerHealth = other.GetComponent<PlayerHealth>();

            if (exitSequenceCoroutine != null)
            {
                StopCoroutine(exitSequenceCoroutine);
                exitSequenceCoroutine = null;
            }

            if (attackSequenceCoroutine == null)
            {
                attackSequenceCoroutine = StartCoroutine(AttackSequenceRoutine());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isDead) return;

        if (other.CompareTag("Player"))
        {
            playerHealth = null;

            if (attackSequenceCoroutine != null)
            {
                StopCoroutine(attackSequenceCoroutine);
                attackSequenceCoroutine = null;
            }

            if (exitSequenceCoroutine == null)
            {
                exitSequenceCoroutine = StartCoroutine(ExitAttackRoutine());
            }
        }
    }

    private IEnumerator AttackSequenceRoutine()
    {
        PlayAnimation(attackStartAnimName);

        yield return new WaitForSeconds(attackStartDuration);

        if (isDead) yield break;

        PlayAnimation(attackLoopAnimName);
        SpawnPoisonVFX();

        while (playerHealth != null && !isDead)
        {
            playerHealth.TakeDamage(poisonDamage, transform.position, 0f);
            yield return new WaitForSeconds(damageInterval);
        }
    }

    private IEnumerator ExitAttackRoutine()
    {
        DestroyPoisonVFX();

        if (isDead) yield break;

        PlayAnimation(attackEndAnimName);

        yield return new WaitForSeconds(attackEndDuration);

        if (!isDead)
        {
            PlayAnimation(idleAnimName);
        }

        exitSequenceCoroutine = null;
    }

    public void PlayDeathAnimation()
    {
        isDead = true;

        // Bricht sofort alle aktiven Coroutinen ab
        StopAllCoroutines();
        attackSequenceCoroutine = null;
        exitSequenceCoroutine = null;

        DestroyPoisonVFX();

        // Spielt exakt die Tod-Animation ab
        PlayAnimation(deathAnimName);

        // Deaktiviert den Collider, damit gar keine Triggers mehr auslösen können
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    private void SpawnPoisonVFX()
    {
        if (poisonVFXPrefab != null && vfxSpawnPoint != null && currentVFXInstance == null && !isDead)
        {
            currentVFXInstance = Instantiate(poisonVFXPrefab, vfxSpawnPoint.position, vfxSpawnPoint.rotation, vfxSpawnPoint);
        }
    }

    private void DestroyPoisonVFX()
    {
        if (currentVFXInstance != null)
        {
            ParticleSystem ps = currentVFXInstance.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Stop();
            }
            Destroy(currentVFXInstance, 2.0f);
            currentVFXInstance = null;
        }
    }

    private void PlayAnimation(string animName)
    {
        if (animator == null || string.IsNullOrEmpty(animName)) return;

        // ABSOLUTE SPERRE: Wenn tot, darf NUR NOCH die Todesanimation abgespielt werden!
        if (isDead && animName != deathAnimName) return;

        int stateHash = Animator.StringToHash(animName);
        if (!animator.HasState(0, stateHash))
        {
            Debug.LogError($"[PFLANZE] FEHLER: State '{animName}' existiert NICHT im Animator Controller!");
            return;
        }

        if (currentAnimation != animName)
        {
            currentAnimation = animName;
            animator.CrossFade(animName, 0.1f);
        }
    }
}