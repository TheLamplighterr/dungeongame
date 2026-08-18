using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class SlimeAI : BaseEnemyAI
{
    public Animator animator;

    private EnemyDamage enemyDamage;

    [Header("Impact")]
    public GameObject impactEffect;
    public Transform impactPoint;

    [Header("Jump Attack Settings")]
    [Tooltip("Wie schnell der Slime während des Sprungs nach vorne fliegt")]
    public float jumpSpeed = 8f;

    [Header("Kollisions-Stopp")]
    [Tooltip("Ab diesem Abstand hält der Slime an, um nicht in den Spieler reinzulaufen")]
    public float stopDistanceToPlayer = 2f;

    private bool isAttacking;

    protected override void Awake()
    {
        base.Awake();

        animator = GetComponent<Animator>();
        enemyDamage = GetComponent<EnemyDamage>();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        isAttacking = false;
        canAct = false;
    }

    protected override void Idle()
    {
        base.Idle();

        if (isAttacking) return;

        if (animator != null)
            animator.CrossFade("Slime_Idle_BAKED", 0.1f);
    }

    protected override void Chase()
    {
        base.Chase();

        if (isAttacking) return;

        // Verhindert das Wegschieben und Reinschieben
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null && agent != null && agent.isOnNavMesh)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerObj.transform.position);

            if (distanceToPlayer <= stopDistanceToPlayer)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero; // Stoppt jeglichen Bewegungsschwung sofort
            }
            else
            {
                agent.isStopped = false;
            }
        }

        if (animator != null)
            animator.CrossFade("Slime_Idle_BAKED", 0.1f);
    }

    protected override void Attack()
    {
        base.Attack();

        if (isAttacking) return;

        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        canAct = false;

        // 1. NavMeshAgent stoppen
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        // 2. Zielposition bestimmen
        Vector3 targetPosition = transform.position;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            targetPosition = playerObj.transform.position;
        }

        targetPosition.y = transform.position.y;

        // Zum Ziel drehen
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // 3. Sprung vorbereiten
        if (animator != null)
            animator.Play("Slime_JumpStart_BAKED", 0, 0f);

        yield return new WaitForSeconds(0.3f);

        // 4. Sprung ausführen
        if (animator != null)
            animator.Play("Slime_JumpLoop_BAKED", 0, 0f);

        float jumpDuration = 1.2f;
        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;

        while (elapsedTime < jumpDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / jumpDuration;

            transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        if (agent != null && agent.isOnNavMesh)
        {
            agent.Warp(transform.position);
        }

        // 5. Einschlag & Schaden
        SpawnImpact();

        if (enemyDamage != null && enemyDamage.enabled)
            enemyDamage.DealDamage();

        if (animator != null)
            animator.Play("Slime_JumpEnd_BAKED", 0, 0f);

        yield return new WaitForSeconds(0.3f);

        if (animator != null)
            animator.Play("Slime_Idle_BAKED");

        yield return new WaitForSeconds(1f);

        isAttacking = false;
        canAct = true;
    }

    void SpawnImpact()
    {
        if (impactEffect == null)
            return;

        Vector3 spawnPos = impactPoint != null ? impactPoint.position : transform.position;
        spawnPos += Vector3.up * 0.2f;

        GameObject fx = Instantiate(
            impactEffect,
            spawnPos,
            Quaternion.Euler(90f, 0f, 0f)
        );

        fx.transform.localScale = Vector3.one * 2f;

        Debug.Log("[SlimeAI] Impact spawned");
    }
    
}