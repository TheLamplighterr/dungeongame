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

    [Header("Distance Settings")]
    [Tooltip("Ab diesem Abstand hält der Golem an, um nicht in den Spieler reinzudrücken")]
    public float stopDistanceToPlayer = 3.0f;

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

    protected void Start()
    {
        // Automatisch den Spieler in der Szene über den Tag suchen (für Prefabs)
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                player = p.transform;
            }
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        isAttacking = false;
        canAct = false;
    }

    protected override void Update()
    {
        // Fallback: Falls der Spieler verzögert/später gespawnt wird
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                player = p.transform;
            }
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

        if (player != null && agent != null && agent.isOnNavMesh)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // Wenn wir in Schlagreichweite sind, überlassen wir der BaseEnemyAI den Angriff
            if (distanceToPlayer <= attackRange)
            {
                return;
            }

            // Stoppen außerhalb der Attack-Range verhindern
            if (distanceToPlayer <= stopDistanceToPlayer)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;

                if (currentAnimation != "Idle")
                {
                    currentAnimation = "Idle";
                    animator.CrossFade("Golem_Idle", 0.2f);
                }
                return;
            }
            else
            {
                agent.isStopped = false;
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
        canAct = false;

        // Movement hart stoppen
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.ResetPath();
        }

        // Drehung zum Spieler ausrichten
        if (player != null)
        {
            Vector3 lookPos = player.position - transform.position;
            lookPos.y = 0;
            if (lookPos != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(lookPos);
            }
        }

        currentAnimation = "Attack";

        // Angriffsanimation abspielen
        if (animator != null)
            animator.Play("Golem_Attack", 0, 0f);

        // Treffer-Zeitpunkt abwarten
        yield return new WaitForSeconds(attackHitTime);

        // Treffer ausführen
        SpawnImpact();

        if (enemyDamage != null)
            enemyDamage.DealDamage();

        // Übergang ins Idle
        if (animator != null)
        {
            currentAnimation = "Idle";
            animator.CrossFade("Golem_Idle", 0.2f);
        }

        // Cooldown abwarten
        yield return new WaitForSeconds(attackCooldown);

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