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

    [Header("Timings & Cooldowns")]
    public float activateDuration = 3.75f;
    public float attackDuration = 1.0f;

    [Tooltip("Zeitpunkt im Schlag, an dem der Schaden zugefügt wird (z. B. 0.4s nach Animationsstart)")]
    public float damageTiming = 0.4f;

    public float minAttackCooldown = 1.5f;
    public float maxAttackCooldown = 3.0f;

    [Header("Kollisions-Stopp")]
    [Tooltip("Ab diesem Abstand hält der Lampling an, um nicht in den Spieler reinzulaufen")]
    public float stopDistanceToPlayer = 2f;

    private bool isActivating;
    private bool isActivated;
    private bool isAttacking;
    private bool isOnCooldown;

    protected override void Awake()
    {
        base.Awake();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        enemyDamage = GetComponent<EnemyDamage>();
        if (enemyDamage == null)
            enemyDamage = GetComponentInChildren<EnemyDamage>();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        isAttacking = false;
        isActivating = false;
        canAct = false;
    }

    protected override void Idle()
    {
        base.Idle();

        if (isAttacking || isActivating) return;

        PlayAnim(idleState);
    }

    protected override void Chase()
    {
        base.Chase();

        if (isAttacking || !isActivated) return;

        // Verhindert das Wegschieben und Reinschieben (exakt wie beim Slime)
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

        LookAtPlayer();
        PlayAnim(idleState);
    }

    protected override void Update()
    {
        // 1. Aufwach-Phase
        if (!isActivated)
        {
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

        base.Update();
    }

    private IEnumerator ActivateSequence()
    {
        isActivating = true;
        canAct = false;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        PlayAnim(activateState, true);

        yield return new WaitForSeconds(activateDuration);

        PlayAnim(idleState, true);
        LookAtPlayer();

        isActivated = true;
        isActivating = false;
        canAct = true;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }
    }

    protected override void Attack()
    {
        base.Attack();

        if (isAttacking || isOnCooldown || !canAct) return;

        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        isOnCooldown = true;

        // 1. Agent stoppen & ausrichten
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }

        LookAtPlayer();
        PlayAnim(strikeState, true);

        // 2. Warten bis zum Treffer-Moment der Schlag-Animation
        yield return new WaitForSeconds(damageTiming);

        // 3. Schaden verteilen (Direkter Aufruf an EnemyDamage)
        if (enemyDamage != null && enemyDamage.enabled)
        {
            enemyDamage.DealDamage();
        }

        SpawnImpact();

        // 4. Restliche Animationszeit abwarten
        float remainingTime = Mathf.Max(0f, attackDuration - damageTiming);
        yield return new WaitForSeconds(remainingTime);

        PlayAnim(idleState, true);
        isAttacking = false;

        // 5. Cooldown
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

    private void LookAtPlayer()
    {
        if (player == null) return;

        Vector3 dir = (player.position - transform.position);
        dir.y = 0;

        if (dir.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 8f);
        }
    }
}