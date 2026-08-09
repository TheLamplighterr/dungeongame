using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    public int damage = 10;
    public float attackRange = 2.5f;

    [Header("Knockback")]
    [Tooltip("Stärke des Rückstoßes, den der Spieler bei diesem Angriff erleidet")]
    public float knockbackForce = 10f;

    Transform player;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");

        if (p != null)
            player = p.transform;
    }

    public void DealDamage()
    {
        // Falls der Player beim Start noch nicht gefunden wurde
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            else return;
        }

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        if (distance > attackRange)
        {
            Debug.Log("[EnemyDamage] Player out of range");
            return;
        }

        PlayerHealth health = player.GetComponent<PlayerHealth>();

        if (health != null)
        {
            // Sendet Schaden, eigene Position (für die Stoßrichtung) & Knockback-Stärke
            health.TakeDamage(damage, transform.position, knockbackForce);

            Debug.Log(
                "[EnemyDamage] Hit player for " +
                damage +
                " damage with knockback (" + knockbackForce + ")"
            );
        }
    }
}