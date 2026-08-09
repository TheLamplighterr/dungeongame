using UnityEngine;
using UnityEngine.Events;

public class DestroyableTarget : MonoBehaviour
{
    [Header("Ziel Einstellungen")]
    [SerializeField] private int maxHealth = 1;
    private int currentHealth;

    [Header("Visuelles Feedback & Effekte")]
    [SerializeField] private GameObject destroyVFXPrefab; // Partikeleffekt beim Zerstören
    [SerializeField] private AudioClip hitSound;          // Treffer-Sound
    [SerializeField] private GameObject visualsGroup;     // Unterobjekt 'Visuals' (wird verborgen)

    [Header("Optionales Rätsel-Event")]
    public UnityEvent OnTargetDestroyed;

    private bool isDestroyed = false;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (isDestroyed) return;

        currentHealth -= damage;

        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position);
        }

        if (currentHealth <= 0)
        {
            DestroyTarget();
        }
    }

    private void DestroyTarget()
    {
        isDestroyed = true;

        if (destroyVFXPrefab != null)
        {
            Instantiate(destroyVFXPrefab, transform.position, Quaternion.identity);
        }

        OnTargetDestroyed?.Invoke();

        // Modell verbergen
        if (visualsGroup != null)
        {
            visualsGroup.SetActive(false);
        }

        // Collider deaktivieren
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Objekt aufräumen
        Destroy(gameObject, 2f);
    }
}