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

    private Transform mainCameraTransform;

    void Start()
    {
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }

        if (potionPrefab == null)
        {
            Debug.LogWarning("PotionThrower: Kein 'potionPrefab' zugewiesen! Der Trank-Wurf wird nicht visuell sichtbar sein.");
        }
    }

    /// <summary>
    /// Diese Funktion wird von außen aufgerufen, wenn der Wurf ausgeführt wird.
    /// </summary>
    public void Throw()
    {
        if (potionPrefab == null || mainCameraTransform == null)
            return;

        // Startpunkt festlegen (falls kein Origin zugewiesen ist, nehmen wir die Spielerposition leicht erhöht)
        Vector3 spawnPosition = throwOrigin != null ? throwOrigin.position : transform.position + Vector3.up * 1.5f;

        // Trank instanziieren (erzeugen)
        GameObject activePotion = Instantiate(potionPrefab, spawnPosition, Quaternion.identity);

        // Physik-Komponente holen
        Rigidbody rb = activePotion.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Wir werfen den Trank genau in Blickrichtung der Kamera!
            Vector3 throwDirection = mainCameraTransform.forward;
            
            // Kraftvektor berechnen (Kamera-Blickrichtung + leichter Bogen nach oben)
            Vector3 forceToApply = (throwDirection * throwForce) + (Vector3.up * upwardForce);

            // Kraft physikalisch hinzufügen
            rb.AddForce(forceToApply, ForceMode.Impulse);
        }
        else
        {
            Debug.LogError("PotionThrower: Das Potion-Prefab benötigt eine 'Rigidbody'-Komponente, um geworfen zu werden!");
        }
    }
}