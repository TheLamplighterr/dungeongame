using UnityEngine;
using UnityEngine.AI;

public class BaseEnemyAI : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public Transform player;

    [Header("Ranges")]
    public float sightRange = 10f;
    public float attackRange = 2f;

    protected bool canAct = true;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
    }

    protected virtual void Update()
    {
        if (player == null || agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        if (!canAct)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > sightRange)
        {
            Idle();
        }
        else if (distance > attackRange)
        {
            Chase();
        }
        else
        {
            Attack();
        }
    }

    protected virtual void Idle()
    {
        if (!agent.isOnNavMesh) return;

        agent.isStopped = true;
        agent.ResetPath();
    }

    protected virtual void Chase()
    {
        if (!agent.isOnNavMesh) return;

        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    protected virtual void Attack()
    {
        agent.isStopped = true;
    }
}