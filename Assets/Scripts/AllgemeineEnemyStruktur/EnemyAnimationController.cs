using UnityEngine;
using UnityEngine.AI;

public class EnemyAnimationController : MonoBehaviour
{
    public Animator animator;
    public BaseEnemyAI ai;
    public NavMeshAgent agent;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        ai = GetComponent<BaseEnemyAI>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        HandleMovement();
        HandleAttack();
    }

    void HandleMovement()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        bool isMoving = agent.velocity.magnitude > 0.1f;
        animator.SetBool("IsMoving", isMoving);
    }

    void HandleAttack()
    {
        // ganz simpel erstmal:
        bool isAttacking = agent.isStopped && ai != null;

        animator.SetBool("IsAttacking", isAttacking);
    }
}