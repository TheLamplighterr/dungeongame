using UnityEngine;

public class PotionProjectile : MonoBehaviour
{
    [Header("VFX & Sound")]
    [Tooltip("Der Partikeleffekt, der beim Aufprall spawnen soll (z. B. eine bunte Explosion).")]
    [SerializeField] private GameObject impactVFXPrefab;

    [Header("Aufprall-Audio")]
    [Tooltip("Der Sound, der beim Einschlag gespielt wird (z. B. Glasbrechen oder Magie-Explosion).")]
    [SerializeField] private AudioClip impactSound;
    [Tooltip("Die Lautstärke des Aufprall-Sounds (0.0 bis 1.0).")]
    [Range(0f, 1f)]
    [SerializeField] private float soundVolume = 1f;

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

        // 1. AUFPRAALL-SOUND SPIELEN
        // PlayClipAtPoint erstellt ein temporäres Audio-Objekt an der Stelle des Einschlags,
        // sodass der Sound sauber zu Ende spielt, selbst wenn der Trank in der nächsten Zeile zerstört wird!
        if (impactSound != null)
        {
            AudioSource.PlayClipAtPoint(impactSound, spawnPosition, soundVolume);
        }

        // 2. VFX SPANWEN
        if (impactVFXPrefab != null)
        {
            GameObject vfxInstance = Instantiate(impactVFXPrefab, spawnPosition, spawnRotation);
            Destroy(vfxInstance, 3f);
        }

        // 3. FLÄCHENSCHADEN AN GEGNER VERTEILEN
        DealExplosionDamage(spawnPosition);

        // 4. TRANK-PREFAB LÖSCHEN
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