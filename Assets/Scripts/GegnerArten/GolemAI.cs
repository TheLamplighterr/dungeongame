using UnityEngine;
using System.Collections;

public class GolemAI : BaseEnemyAI
{
    public Animator animator;

    private EnemyDamage enemyDamage;
    private EnemyHealth enemyHealth;

    [Header("Boss Settings")]
    public float moveSpeed = 2f;
    [Tooltip("Normale Drehgeschwindigkeit beim Laufen")]
    public float turnSpeed = 5f;
    [Tooltip("Schnellere Drehgeschwindigkeit im Cooldown/Idle")]
    public float cooldownTurnSpeed = 14f;

    public float attackHitTime = 1.0f;
    public float attackCooldown = 2.0f;

    [Header("Distance Settings")]
    [Tooltip("Ab diesem Abstand hält der Golem an, um nicht in den Spieler reinzudrücken")]
    public float stopDistanceToPlayer = 3.0f;

    [Header("Idle Timeout")]
    public float idleTimeBeforeCancelUI = 3.0f;

    [Header("Impact")]
    public GameObject impactEffect;
    public Transform impactPoint;

    private bool isAttacking;
    private bool isOnCooldown;
    private bool hasTriggeredBossUI = false;
    private Coroutine cancelUICoroutine;

    private string currentAnimation = "";

    protected override void Awake()
    {
        base.Awake();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator != null)
            animator.applyRootMotion = false;

        enemyDamage = GetComponent<EnemyDamage>();
        enemyHealth = GetComponent<EnemyHealth>();

        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.updateRotation = false;
        }
    }

    protected void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        isAttacking = false;
        isOnCooldown = false;
        canAct = false;
    }

    protected override void Update()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (agent != null && agent.updateRotation)
        {
            agent.updateRotation = false;
        }

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
        }
    }

    protected override void Idle()
    {
        base.Idle();

        if (isAttacking || animator == null)
            return;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            if (agent.hasPath) agent.ResetPath();
        }

        LookAtPlayerSmooth(isOnCooldown ? cooldownTurnSpeed : turnSpeed);
        PlayIdleAnim();

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

        // WÄHREND DES COOLDOWNS: Nicht laufen, sondern im Idle zügig zum Spieler drehen!
        if (isOnCooldown)
        {
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }

            LookAtPlayerSmooth(cooldownTurnSpeed);
            PlayIdleAnim();
            return;
        }

        if (player != null && agent != null && agent.isOnNavMesh)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= stopDistanceToPlayer)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;

                LookAtPlayerSmooth(turnSpeed);
                PlayIdleAnim();
                return;
            }
            else
            {
                agent.isStopped = false;
                LookAtPlayerSmooth(turnSpeed);
            }
        }

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

        // Während des Cooldowns im Idle-State bleiben und mitdrehen
        if (isOnCooldown)
        {
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }

            LookAtPlayerSmooth(cooldownTurnSpeed);
            PlayIdleAnim();
            return;
        }

        if (isAttacking)
            return;

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

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.ResetPath();
        }

        LookAtPlayerInstant();

        currentAnimation = "Attack";

        if (animator != null)
            animator.Play("Golem_Attack", 0, 0f);

        yield return new WaitForSeconds(attackHitTime);

        SpawnImpact();

        if (enemyDamage != null)
            enemyDamage.DealDamage();

        PlayIdleAnim(0.2f);

        // SCHLAG BEENDET: Cooldown beginnt
        isAttacking = false;
        isOnCooldown = true;

        // Während dieser Phase steht er im Idle und dreht sich flüssig mit!
        yield return new WaitForSeconds(attackCooldown);

        isOnCooldown = false;
    }

    private void PlayIdleAnim(float fadeTime = 0.1f)
    {
        if (currentAnimation != "Idle" && animator != null)
        {
            currentAnimation = "Idle";
            animator.CrossFade("Golem_Idle", fadeTime);
        }
    }

    private void LookAtPlayerSmooth(float speed)
    {
        if (player == null) return;

        Vector3 dir = (player.position - transform.position);
        dir.y = 0;

        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed);
        }
    }

    private void LookAtPlayerInstant()
    {
        if (player == null) return;

        Vector3 dir = (player.position - transform.position);
        dir.y = 0;

        if (dir.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    void SpawnImpact()
    {
        if (impactEffect == null) return;
        Vector3 spawnPos = impactPoint != null ? impactPoint.position : transform.position;
        Instantiate(impactEffect, spawnPos, Quaternion.identity);
    }

    private void OnDestroy()
{
    // Prüfen, ob der Golem wirklich gestorben ist (HP <= 0) 
    // und nicht nur die Szene neu geladen wurde:
    if (enemyHealth != null && enemyHealth.currentHealth <= 0)
    {
        MapManager mapManager = FindObjectOfType<MapManager>();
        if (mapManager != null)
        {
            mapManager.BossDefeat();
        }
    }
}
}