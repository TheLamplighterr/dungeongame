using UnityEngine;

public class PlayerCrosshairUI : MonoBehaviour
{
    [Header("Zuweisung")]
    [Tooltip("Das UI-Bild (Crosshair) aus deinem Canvas.")]
    [SerializeField] private GameObject crosshairVisual;

    private PlayerAttack playerAttack;

    void Start()
    {
        // Sucht die PlayerAttack-Komponente auf demselben Objekt oder den Parents
        playerAttack = GetComponentInParent<PlayerAttack>();

        if (playerAttack == null)
        {
            Debug.LogWarning("PlayerCrosshairUI: Kein PlayerAttack-Skript auf dem Spieler gefunden!");
        }

        // Standardmäßig ausblenden zum Start
        if (crosshairVisual != null)
        {
            crosshairVisual.SetActive(false);
        }
    }

    void Update()
    {
        // Falls eines der Objekte fehlt, brechen wir geräuschlos ab, damit im Spiel nichts abstürzt!
        if (playerAttack == null || crosshairVisual == null)
            return;

        // Wir prüfen, ob der Spieler zielt
        bool isCurrentlyAiming = playerAttack.IsAiming();

        // Wenn der Zustand des Fadenkreuzes nicht mit dem Aiming-Zustand übereinstimmt, passen wir ihn an
        if (crosshairVisual.activeSelf != isCurrentlyAiming)
        {
            crosshairVisual.SetActive(isCurrentlyAiming);
        }
    }
}