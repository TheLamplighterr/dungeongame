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
    [Tooltip("Der Radius der Explosion. Alle Objekte im Radius erleiden Schaden / werden aktiviert.")]
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

        // 3. FLÄCHENSCHADEN AN GEGNER, ZIELSCHAIBEN & SCHALTER VERTEILEN
        DealExplosionDamage(spawnPosition);

        // 4. TRANK-PREFAB LÖSCHEN
        Destroy(gameObject);
    }

    private void DealExplosionDamage(Vector3 explosionPoint)
    {
        // A) GEGNER IM ENEMY-LAYER PRÜFEN
        Collider[] hitEnemies = Physics.OverlapSphere(explosionPoint, explosionRadius, enemyLayer);
        foreach (Collider enemyCollider in hitEnemies)
        {
            EnemyHealth enemy = enemyCollider.GetComponent<EnemyHealth>();
            if (enemy == null) enemy = enemyCollider.GetComponentInParent<EnemyHealth>();
            
            if (enemy != null)
            {
                enemy.TakeDamage(potionDamage);
                Debug.Log($"[Potion-Einschlag] Gegner {enemyCollider.name} hat {potionDamage} Schaden erlitten!");
            }
        }

        // B) ALLE COLLIDER IM RADIUS FÜR ZIELSCHAIBEN & SCHALTER PRÜFEN (Layer-unabhängig)
        Collider[] allHits = Physics.OverlapSphere(explosionPoint, explosionRadius);
        foreach (Collider hit in allHits)
        {
            // 1. Zerstörbares Ziel prüfen
            DestroyableTarget destroyable = hit.GetComponent<DestroyableTarget>();
            if (destroyable == null) destroyable = hit.GetComponentInParent<DestroyableTarget>();

            if (destroyable != null)
            {
                destroyable.TakeDamage(potionDamage);
                Debug.Log($"[Potion-Einschlag] Zielscheibe {hit.name} getroffen!");
                continue;
            }

            // 2. Kristall-Schalter (z. B. an Decke/Wand) prüfen
            CrystalSwitch crystal = hit.GetComponent<CrystalSwitch>();
            if (crystal == null) crystal = hit.GetComponentInParent<CrystalSwitch>();

            if (crystal != null)
            {
                crystal.TakeDamage(potionDamage);
                Debug.Log($"[Potion-Einschlag] Kristall-Schalter {hit.name} aktiviert!");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}