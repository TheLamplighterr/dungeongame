using UnityEngine;

public class PotionThrower : MonoBehaviour
{
    [Header("Trank-Prefab")]
    [Tooltip("Das Prefab des Tranks, das geworfen werden soll.")]
    [SerializeField] private GameObject potionPrefab;

    [Header("Wurf-Einstellungen")]
    [Tooltip("Die Stelle, an der der Trank in der Hand des Spielers startet.")]
    [SerializeField] private Transform throwOrigin;
    [SerializeField] private float throwForce = 15f;
    [SerializeField] private float upwardForce = 2f;

    [Header("Audio (Wurf)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip throwSound; // Wurf-Swoosh Sound

    private Transform mainCameraTransform;

    void Start()
    {
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (potionPrefab == null)
        {
            Debug.LogWarning("PotionThrower: Kein 'potionPrefab' zugewiesen! Der Trank-Wurf wird nicht visuell sichtbar sein.");
        }
    }

    public void Throw()
    {
        if (potionPrefab == null || mainCameraTransform == null)
            return;

        Vector3 spawnPosition = throwOrigin != null ? throwOrigin.position : transform.position + Vector3.up * 1.5f;

        GameObject activePotion = Instantiate(potionPrefab, spawnPosition, Quaternion.identity);

        // --- AUDIO: Wurf-Sound abspielen ---
        if (audioSource != null && throwSound != null)
        {
            audioSource.PlayOneShot(throwSound);
        }

        Rigidbody rb = activePotion.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 throwDirection = mainCameraTransform.forward;
            Vector3 forceToApply = (throwDirection * throwForce) + (Vector3.up * upwardForce);

            rb.AddForce(forceToApply, ForceMode.Impulse);
        }
        else
        {
            Debug.LogError("PotionThrower: Das Potion-Prefab benötigt eine 'Rigidbody'-Komponente, um geworfen zu werden!");
        }
    }
}