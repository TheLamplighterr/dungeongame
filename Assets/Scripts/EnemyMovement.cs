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

    [Header("Patrol")]
    public float walkPointRange = 10f;

    [Header("Ranges")]
    public float sightRange = 15f;
    public float attackRange = 4f;

    [Header("Combat")]
    public int attackDamage = 20;
    public float attackRadius = 3f;
    public float timeBetweenAttacks = 2f;

    // INTERNAL
    private Vector3 walkPoint;
    private bool walkPointSet;

    private bool playerInSightRange;
    private bool playerInAttackRange;

    private bool alreadyAttacked;

    void Awake()
    {
        // PLAYER SUCHEN
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("Kein Objekt mit Tag 'Player' gefunden!");
        }

        // NAVMESH AGENT
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError("Kein NavMeshAgent gefunden!");
            return;
        }

        // AGENT SETTINGS
        agent.updateRotation = true;
        agent.autoBraking = true;

        // Wichtig gegen jitter
        agent.stoppingDistance = attackRange - 1f;

        // Smooth movement
        agent.acceleration = 8f;
        agent.angularSpeed = 120f;
    }

    void Update()
    {
        if (player == null)
            return;

        // RANGE CHECKS
        playerInSightRange = Physics.CheckSphere(
            transform.position,
            sightRange,
            whatIsPlayer
        );

        playerInAttackRange = Physics.CheckSphere(
            transform.position,
            attackRange,
            whatIsPlayer
        );

        // STATES
        if (!playerInSightRange && !playerInAttackRange)
        {
            Patrol();
        }
        else if (playerInSightRange && !playerInAttackRange)
        {
            ChasePlayer();
        }
        else if (playerInAttackRange && playerInSightRange)
        {
            AttackPlayer();
        }
    }

    // ==================================================
    // PATROL
    // ==================================================

    void Patrol()
    {
        if (!walkPointSet)
        {
            SearchWalkPoint();
        }

        if (walkPointSet)
        {
            agent.isStopped = false;
            agent.SetDestination(walkPoint);
        }

        // Ziel erreicht
        if (!agent.pathPending && agent.remainingDistance < 1f)
        {
            walkPointSet = false;
        }
    }

    void SearchWalkPoint()
    {
        float randomX = Random.Range(-walkPointRange, walkPointRange);
        float randomZ = Random.Range(-walkPointRange, walkPointRange);

        Vector3 potentialPoint = new Vector3(
            transform.position.x + randomX,
            transform.position.y,
            transform.position.z + randomZ
        );

        // Punkt auf Boden prüfen
        if (Physics.Raycast(potentialPoint, Vector3.down, 2f, whatIsGround))
        {
            walkPoint = potentialPoint;
            walkPointSet = true;
        }
    }

    // ==================================================
    // CHASE
    // ==================================================

    void ChasePlayer()
    {
        agent.isStopped = false;

        // Nicht direkt in den Spieler laufen
        Vector3 dirToPlayer =
            (transform.position - player.position).normalized;

        Vector3 targetPos =
            player.position + dirToPlayer * (attackRange - 1f);

        agent.SetDestination(targetPos);
    }

    // ==================================================
    // ATTACK
    // ==================================================

    void AttackPlayer()
    {
        // Agent stoppen
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        // Keine neue Bewegung berechnen
        agent.SetDestination(transform.position);

        // Smooth rotation
        Vector3 lookDirection =
            (player.position - transform.position).normalized;

        lookDirection.y = 0;

        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(lookDirection);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * 5f
            );
        }

        // Safety Check
        if (attackPoint == null)
        {
            Debug.LogWarning("AttackPoint fehlt!");
            return;
        }

        // Attack
        if (!alreadyAttacked)
        {
            Debug.Log("Enemy attacks!");

            // Effekt
            if (groundImpactEffect != null)
            {
                groundImpactEffect.Play();
            }

            // Spieler im Radius finden
            Collider[] hitPlayers = Physics.OverlapSphere(
                attackPoint.position,
                attackRadius,
                whatIsPlayer
            );

            foreach (Collider hit in hitPlayers)
            {
                Debug.Log("Player hit!");

                // Später:
                // PlayerHealth hp =
                //     hit.GetComponent<PlayerHealth>();

                // if (hp != null)
                // {
                //     hp.TakeDamage(attackDamage);
                // }
            }

            alreadyAttacked = true;

            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    void ResetAttack()
    {
        alreadyAttacked = false;
    }

    // ==================================================
    // DAMAGE
    // ==================================================

    public void TakeDamage(int damage)
    {
        Debug.Log("Enemy took damage!");
    }

    public void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    // ==================================================
    // GIZMOS
    // ==================================================

    void OnDrawGizmosSelected()
    {
        // Sight Range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        // Attack Range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Echter Attack Radius
        if (attackPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(
                attackPoint.position,
                attackRadius
            );
        }
    }
}