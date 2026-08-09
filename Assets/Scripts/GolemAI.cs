using UnityEngine;
using System.Collections;

public class GolemAI : BaseEnemyAI
{
    public Animator animator;

    private EnemyDamage enemyDamage;
    private EnemyHealth enemyHealth;

    [Header("Boss Settings")]
    public float moveSpeed = 2f;
    public float attackHitTime = 1.0f;
    public float attackCooldown = 1.5f;

    [Header("Idle Timeout")]
    [Tooltip("Wie lange der Golem ungestört im Idle sein muss, bis die Boss-UI verschwindet")]
    public float idleTimeBeforeCancelUI = 3.0f;

    [Header("Impact")]
    public GameObject impactEffect;
    public Transform impactPoint;

    private bool isAttacking;
    private bool hasTriggeredBossUI = false;
    private Coroutine cancelUICoroutine;

    private string currentAnimation = "";

    protected override void Awake()
    {
        base.Awake();

        animator = GetComponentInChildren<Animator>();
        enemyDamage = GetComponent<EnemyDamage>();
        enemyHealth = GetComponent<EnemyHealth>();

        if (agent != null)
            agent.speed = moveSpeed;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        isAttacking = false;
        canAct = false;
    }

    protected override void Update()
    {
        base.Update();

        if (player != null && agent != null && agent.enabled && enemyHealth != null && enemyHealth.currentHealth > 0)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (!hasTriggeredBossUI && distance <= sightRange)
            {
                TriggerBossFightUI();
            }
        }
    }

    private void TriggerBossFightUI()
    {
        if (cancelUICoroutine != null)
        {
            StopCoroutine(cancelUICoroutine);
            cancelUICoroutine = null;
        }

        if (BossUI.Instance != null && enemyHealth != null)
        {
            BossUI.Instance.StartBossFight(enemyHealth);
            hasTriggeredBossUI = true;
            Debug.Log($"[BOSS-AI] Bosskampf-UI für {gameObject.name} gestartet!");
        }
    }

    protected override void Idle()
    {
        base.Idle();

        if (isAttacking || animator == null)
            return;

        if (currentAnimation != "Idle")
        {
            currentAnimation = "Idle";
            animator.CrossFade("Golem_Idle", 0.1f);
        }

        if (hasTriggeredBossUI && cancelUICoroutine == null)
        {
            cancelUICoroutine = StartCoroutine(CancelUIAfterIdleDelay());
        }
    }

    protected override void Chase()
    {
        base.Chase();

        if (cancelUICoroutine != null)
        {
            StopCoroutine(cancelUICoroutine);
            cancelUICoroutine = null;
        }

        if (isAttacking || animator == null)
            return;

        if (currentAnimation != "Walk")
        {
            currentAnimation = "Walk";
            animator.CrossFade("Golem_Walk", 0.15f);
        }
    }

    protected override void Attack()
    {
        base.Attack();

        if (cancelUICoroutine != null)
        {
            StopCoroutine(cancelUICoroutine);
            cancelUICoroutine = null;
        }

        if (isAttacking)
            return;

        currentAnimation = "Attack";
        StartCoroutine(AttackRoutine());
    }

    private IEnumerator CancelUIAfterIdleDelay()
    {
        yield return new WaitForSeconds(idleTimeBeforeCancelUI);

        if (BossUI.Instance != null)
        {
            BossUI.Instance.CancelBossFight();
        }

        hasTriggeredBossUI = false;
        cancelUICoroutine = null;
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        canAct = false;

        agent.isStopped = true;
        agent.ResetPath();

        // 1. Angriffsanimation abspielen
        if (animator != null)
            animator.Play("Golem_Attack", 0, 0f);

        // 2. Warten bis zum Trefferzeitpunkt
        yield return new WaitForSeconds(attackHitTime);

        // 3. Treffer ausführen
        SpawnImpact();

        if (enemyDamage != null)
            enemyDamage.DealDamage();

        // --- JETZT NEU: SOFORT INS IDLE WECHSELN ---
        // So bewegt/atmet der Golem während des Cooldowns flüssig, anstatt einzufrieren!
        if (animator != null)
        {
            currentAnimation = "Idle";
            animator.CrossFade("Golem_Idle", 0.2f); // Smooth Übergang
        }

        // 4. Cooldown-Zeit abwarten (Golem befindet sich bereits im Idle)
        yield return new WaitForSeconds(attackCooldown);

        agent.isStopped = false;
        isAttacking = false;
        canAct = true;
    }

    void SpawnImpact()
    {
        if (impactEffect == null)
            return;

        Vector3 spawnPos = impactPoint != null ? impactPoint.position : transform.position;

        Instantiate(
            impactEffect,
            spawnPos,
            Quaternion.identity
        );
    }
}