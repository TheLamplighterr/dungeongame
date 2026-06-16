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

    private bool isAttacking;

    protected override void Awake()
    {
        base.Awake();

        animator = GetComponent<Animator>();
        enemyDamage = GetComponent<EnemyDamage>();
    }

    protected override void Idle()
    {
        base.Idle();

        if (isAttacking) return;

        animator.CrossFade("Slime_Idle_BAKED", 0.1f);
    }

    protected override void Chase()
    {
        base.Chase();

        if (isAttacking) return;
        

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

        agent.isStopped = true;
        agent.ResetPath();

        animator.Play("Slime_JumpStart_BAKED", 0, 0f);
        yield return new WaitForSeconds(0.3f);

        animator.Play("Slime_JumpLoop_BAKED", 0, 0f);
        yield return new WaitForSeconds(1.7f);

        SpawnImpact();

        if (enemyDamage != null)
            enemyDamage.DealDamage();

        animator.Play("Slime_JumpEnd_BAKED", 0, 0f);
        yield return new WaitForSeconds(0.3f);

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