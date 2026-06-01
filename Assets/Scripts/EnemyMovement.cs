using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public Transform player;

    [Header("Attack")]
    public Transform attackPoint;
    public ParticleSystem groundImpactEffect;

    [Header("Layers")]
    public LayerMask whatIsGround;
    public LayerMask whatIsPlayer;

    [Header("Ranges")]
    public float sightRange = 15f;
    public float attackRange = 4f;

    [Header("Combat")]
    public int attackDamage = 20;
    public float attackRadius = 3f;
    public float timeBetweenAttacks = 2f;

    [Header("Debug")]
    public bool debugLogs = true;

    private Vector3 walkPoint;
    private bool walkPointSet;

    private bool playerInSightRange;
    private bool playerInAttackRange;

    private bool alreadyAttacked;

    void Awake()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogError("[Enemy] No Player found!");

        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (player == null) return;

        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange)
            Patrol();
        else if (playerInSightRange && !playerInAttackRange)
            ChasePlayer();
        else if (playerInAttackRange && playerInSightRange)
            AttackPlayer();
    }

    void Patrol()
    {
        agent.isStopped = false;
    }

    void ChasePlayer()
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    void AttackPlayer()
    {
        agent.isStopped = true;

        if (!alreadyAttacked)
        {
            if (debugLogs)
                Debug.Log("[Enemy] ATTACK!");

            if (groundImpactEffect != null)
                groundImpactEffect.Play();

            Collider[] hitPlayers = Physics.OverlapSphere(
                attackPoint.position,
                attackRadius,
                whatIsPlayer
            );

            bool hitOnce = false;

            foreach (Collider hit in hitPlayers)
            {
                if (hitOnce) break;

                PlayerHealth hp = hit.GetComponentInParent<PlayerHealth>();

                if (hp != null)
                {
                    hp.TakeDamage(attackDamage);

                    Debug.Log($"[Enemy] Hit {hit.name} for {attackDamage} damage");

                    hitOnce = true;
                }
                else
                {
                    Debug.LogWarning("[Enemy] No PlayerHealth found on " + hit.name);
                }
            }

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    void ResetAttack()
    {
        alreadyAttacked = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (attackPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}