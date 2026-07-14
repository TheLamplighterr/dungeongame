using UnityEngine;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack")]
    public float attackRange = 3f;
    public int damage = 25;
    public LayerMask enemyLayer;
    public float attackCooldown = 1f;

    [Header("Light Attack")]
    public float lightAttackRange = 3f;
    public int lightDamage = 25;
    public float lightCooldown = 0.7f;

    [Header("Heavy Attack")]
    public float heavyAttackRange = 4f;
    public int heavyDamage = 50;
    public float heavyCooldown = 1.8f;

    private bool canLightAttack = true;
    private bool canHeavyAttack = true;

    public bool canAttack = true;

    // ===== Damage Boost =====
    private Coroutine attackBoostRoutine;
    private int baseDamage;

    // ===== VFX =====
    [Header("VFX")]
    public ParticleSystem damageBoostVFX;


    void Awake()
{
    Debug.Log($"[PlayerHealth] ACTIVE INSTANCE: {gameObject.name} | ID: {GetInstanceID()}");
}

    void Start()
    {
        baseDamage = damage;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && canAttack)
        {
            StartCoroutine(Attack());
        }
    }

    IEnumerator Attack()
    {
        canAttack = false;

        PerformAttack();

        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }

    void PerformAttack()
    {
        Debug.Log($"🗡 Angriff gestartet! Schaden: {damage}");

        Collider[] allHits = Physics.OverlapSphere(transform.position, attackRange);

        Debug.Log("[PlayerAttack] Total colliders found: " + allHits.Length);

        foreach (Collider hit in allHits)
        {
            EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();

            if (enemy != null)
            {
                Debug.Log($" Treffer auf {enemy.name}! Schaden: {damage}");
                enemy.TakeDamage(damage);
            }
        }
    }

    // ==========================
    // DAMAGE BOOST
    // ==========================

    public void BoostDamage(int bonus, float duration)
    {
        if (attackBoostRoutine != null)
        {
            StopCoroutine(attackBoostRoutine);
        }

        attackBoostRoutine = StartCoroutine(DamageBoostRoutine(bonus, duration));
    }

    IEnumerator DamageBoostRoutine(int bonus, float duration)
    {
        damage = baseDamage + bonus;

        // VFX START
        if (damageBoostVFX != null)
        {
            damageBoostVFX.Play();
        }

        Debug.Log($"⚔ Damage Boost aktiv! Schaden: {damage}");

        yield return new WaitForSeconds(duration);

        damage = baseDamage;

        // VFX STOP
        if (damageBoostVFX != null)
        {
            damageBoostVFX.Stop();
        }

        Debug.Log($"⚔ Damage Boost beendet. Schaden: {damage}");

        attackBoostRoutine = null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}