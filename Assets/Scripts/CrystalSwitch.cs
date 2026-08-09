using UnityEngine;
using UnityEngine.Events;

public class CrystalSwitch : MonoBehaviour
{
    [Header("Schalter Einstellungen")]
    [SerializeField] private bool isOneTimeSwitch = true; // Einmalig oder umschaltbar?
    [SerializeField] private bool isActivated = false;

    [Header("Visuelles Feedback")]
    [SerializeField] private Renderer crystalRenderer;   // Der MeshRenderer des Kristalls
    [SerializeField] private Material inactiveMaterial;  // Z.B. Blau
    [SerializeField] private Material activeMaterial;    // Z.B. Grün
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private ParticleSystem hitVFX;

    [Header("Rätsel-Events")]
    public UnityEvent OnActivated;
    public UnityEvent OnDeactivated;

   void Start()
{
    // Falls im Inspector vergessen wurde, den Renderer zuzuweisen,
    // sucht das Skript automatisch im Objekt oder in seinen Kindern danach!
    if (crystalRenderer == null)
    {
        crystalRenderer = GetComponentInChildren<Renderer>();
    }

    UpdateVisuals();
}

    public void TakeDamage(int damage)
    {
        if (isOneTimeSwitch && isActivated) return;

        isActivated = !isActivated;

        if (hitSound != null) AudioSource.PlayClipAtPoint(hitSound, transform.position);
        if (hitVFX != null) hitVFX.Play();

        UpdateVisuals();

        if (isActivated)
        {
            OnActivated?.Invoke();
        }
        else
        {
            OnDeactivated?.Invoke();
        }
    }

    private void UpdateVisuals()
    {
        if (crystalRenderer != null && inactiveMaterial != null && activeMaterial != null)
        {
            crystalRenderer.material = isActivated ? activeMaterial : inactiveMaterial;
        }
    }
}