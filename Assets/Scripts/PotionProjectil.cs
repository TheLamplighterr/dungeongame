using UnityEngine;

public class PotionProjectile : MonoBehaviour
{
    [Header("VFX & Sound")]
    [Tooltip("Der Partikeleffekt, der beim Aufprall spawnen soll (z. B. eine bunte Explosion).")]
    [SerializeField] private GameObject impactVFXPrefab;

    [Header("Schadens-Einstellungen")]
    [Tooltip("Wie viel Schaden macht der Trank bei der Explosion?")]
    [SerializeField] private int potionDamage = 40;
    [Tooltip("Der Radius der Explosion. Alle Gegner im Radius erleiden Schaden.")]
    [SerializeField] private float explosionRadius = 3.5f;
    [Tooltip("Der Layer, auf dem deine Gegner liegen.")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("Einstellungen")]
    [Tooltip("Soll der Trank nach einer bestimmten Zeit sowieso zerstört werden, falls er nichts trifft?")]
    [SerializeField] private float lifetime = 5f;

    private bool hasCollided = false;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasCollided) return;
        hasCollided = true;

        ContactPoint contact = collision.contacts[0];
        Vector3 spawnPosition = contact.point;
        Quaternion spawnRotation = Quaternion.LookRotation(contact.normal);

        if (impactVFXPrefab != null)
        {
            GameObject vfxInstance = Instantiate(impactVFXPrefab, spawnPosition, spawnRotation);
            Destroy(vfxInstance, 3f);
        }

        // FLÄCHENSCHADEN AN Gegner VERTEILEN
        DealExplosionDamage(spawnPosition);

        Destroy(gameObject);
    }

    private void DealExplosionDamage(Vector3 explosionPoint)
    {
        // Findet alle Collider im Explosionsradius, die auf dem Gegner-Layer liegen
        Collider[] hitEnemies = Physics.OverlapSphere(explosionPoint, explosionRadius, enemyLayer);

        foreach (Collider enemyCollider in hitEnemies)
        {
            // Sucht erst direkt auf dem getroffenen Collider nach dem EnemyHealth-Skript
            EnemyHealth enemy = enemyCollider.GetComponent<EnemyHealth>();
            
            // Falls es dort nicht liegt (z.B. weil der Collider auf einem Child-Objekt ist), sucht es im Parent
            if (enemy == null)
            {
                enemy = enemyCollider.GetComponentInParent<EnemyHealth>();
            }
            
            if (enemy != null)
            {
                enemy.TakeDamage(potionDamage);
                Debug.Log($"[Potion-Einschlag] {enemyCollider.name} hat {potionDamage} Schaden erlitten!");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}