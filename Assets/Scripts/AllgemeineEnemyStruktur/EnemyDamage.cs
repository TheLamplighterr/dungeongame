using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    public int damage = 10;
    public float attackRange = 2.5f;

    Transform player;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");

        if (p != null)
            player = p.transform;
    }

    public void DealDamage()
    {
        if (player == null)
            return;


        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        if (distance > attackRange)
        {
            Debug.Log("[EnemyDamage] Player out of range");
            return;
        }

        PlayerHealth health =
            player.GetComponent<PlayerHealth>();

        if (health != null)
        {
            health.TakeDamage(damage);

            Debug.Log(
                "[EnemyDamage] Hit player for " +
                damage +
                " damage"
            );
        }
    }
}