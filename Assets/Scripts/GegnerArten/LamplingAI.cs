using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class LamplingAI : BaseEnemyAI
{
    public Animator animator;
    private EnemyDamage enemyDamage;

    [Header("Impact / FX (Optional)")]
    public GameObject impactEffect;
    public Transform impactPoint;

    [Header("State-Namen im Animator")]
    public string idleState = "Idle";
    public string activateState = "Activate";
    public string strikeState = "Strike";

    [Header("Drehung & Animation")]
    [Tooltip("Normale, gleichmäßige Drehgeschwindigkeit zum Spieler")]
    public float turnSpeed = 8f;

    [Header("Timings & Cooldowns")]
    public float activateDuration = 3.75f;
    public float attackDuration = 1.0f;

    [Tooltip("Zeitpunkt im Schlag, an dem der Schaden zugefügt wird")]
    public float damageTiming = 0.4f;

    public float minAttackCooldown = 1.0f;
    public float maxAttackCooldown = 2.0f;

    [Header("Kollisions-Stopp")]
    public float stopDistanceToPlayer = 2f;

    private bool isActivating;
    private bool isActivated;
    private bool isAttacking;
    private bool isOnCooldown;

    // Neues Flag: Erlaubt die Drehung bereits kurz nach dem Treffer-Moment
    private bool canRotateDuringAttack;

    private Quaternion initialRotation;

    protected override void Awake()
    {
        base.Awake();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator != null)
            animator.applyRootMotion = false;

        enemyDamage = GetComponent<EnemyDamage>();
        if (enemyDamage == null)
            enemyDamage = GetComponentInChildren<EnemyDamage>();

        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updatePosition = true;
        }
    }

    private void Start()
    {
        initialRotation = transform.rotation;

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
        isActivating = false;
        isOnCooldown = false;
        canAct = false;
        canRotateDuringAttack = false;
    }

    protected override void Update()
    {
        if (agent != null && agent.updateRotation)
        {
            agent.updateRotation = false;
        }

        // 1. SCHLAF- & AUFWACH-PHASE
        if (!isActivated)
        {
            transform.rotation = initialRotation;

            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }

            if (!isActivating && player != null)
            {
                float distance = Vector3.Distance(transform.position, player.position);
                if (distance <= sightRange)
                {
                    StartCoroutine(ActivateSequence());
                }
            }

            return;
        }

        // WÄHREND DES ANGRIFFS:
        // Sobald der Schlag ausgeführt wurde (damageTiming vorbei), darf er sich SOFORT flüssig mitdrehen!
        if (isAttacking && canRotateDuringAttack)
        {
            LookAtPlayerSmooth();
        }

        // 2. WACH-PHASE
        base.Update();
    }

    private IEnumerator ActivateSequence()
    {
        isActivating = true;
        canAct = false;

        PlayAnim(activateState, true);

        yield return new WaitForSeconds(activateDuration);

        isActivated = true;
        isActivating = false;
        canAct = true;

        PlayAnim(idleState, true);
        LookAtPlayerSmooth();

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }
    }

    protected override void Idle()
    {
        base.Idle();
        if (isAttacking || isActivating) return;

        LookAtPlayerSmooth();
        PlayAnim(idleState);
    }

    protected override void Chase()
    {
        base.Chase();

        if (isAttacking || !isActivated) return;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null && agent != null && agent.isOnNavMesh)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerObj.transform.position);

            if (distanceToPlayer <= stopDistanceToPlayer)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
            else
            {
                agent.isStopped = false;
            }
        }

        LookAtPlayerSmooth();
        PlayAnim(idleState);
    }

    protected override void Attack()
    {
        base.Attack();

        if (isAttacking || !canAct) return;

        if (isOnCooldown)
        {
            LookAtPlayerSmooth();
            PlayAnim(idleState);
            return;
        }

        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        canRotateDuringAttack = false; // Während dem Ausholen steht die Drehung fest

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }

        SnapToPlayer(); // Direkt vor dem Ausführen exakt zum Spieler ausrichten
        PlayAnim(strikeState, true);

        // 1. Bis zum Schadenspunkt warten
        yield return new WaitForSeconds(damageTiming);

        if (enemyDamage != null && enemyDamage.enabled)
        {
            enemyDamage.DealDamage();
        }

        SpawnImpact();

        // 2. AB JETZT SCHADEN ERFOLGT: Er darf sich im Ausklingen der Animation bereits wieder mitdrehen!
        canRotateDuringAttack = true;

        float remainingTime = Mathf.Max(0f, attackDuration - damageTiming);
        yield return new WaitForSeconds(remainingTime);

        // Schlag beendet
        PlayAnim(idleState, true);
        isAttacking = false;
        canRotateDuringAttack = false;
        isOnCooldown = true;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }

        float currentCooldown = Random.Range(minAttackCooldown, maxAttackCooldown);
        yield return new WaitForSeconds(currentCooldown);

        isOnCooldown = false;
    }

    private void SpawnImpact()
    {
        if (impactEffect == null) return;

        Vector3 spawnPos = impactPoint != null ? impactPoint.position : transform.position;
        spawnPos += Vector3.up * 0.2f;

        GameObject fx = Instantiate(impactEffect, spawnPos, Quaternion.identity);
        fx.transform.localScale = Vector3.one;
    }

    private void PlayAnim(string stateName, bool forcePlay = false)
    {
        if (animator == null || string.IsNullOrEmpty(stateName)) return;

        if (forcePlay)
        {
            animator.Play(stateName, 0, 0f);
        }
        else
        {
            animator.CrossFade(stateName, 0.1f);
        }
    }

    private void LookAtPlayerSmooth()
    {
        if (player == null) return;

        Vector3 dir = (player.position - transform.position);
        dir.y = 0;

        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
        }
    }

    private void SnapToPlayer()
    {
        if (player == null) return;

        Vector3 dir = (player.position - transform.position);
        dir.y = 0;

        if (dir.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }
    }
}