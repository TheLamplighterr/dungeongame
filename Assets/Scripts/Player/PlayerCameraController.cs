using UnityEngine;
using Unity.Cinemachine; // Unity 6 / Cinemachine v3

public class PlayerCameraController : MonoBehaviour
{
    [Header("Kameras")]
    [SerializeField] private CinemachineCamera normalCamera;
    [SerializeField] private CinemachineCamera aimCamera;

    [Header("Kamera Target Einstellungen")]
    [SerializeField] private Transform cameraTarget; // Das gemeinsame Kamera-Target
    
    [Tooltip("Standard-Offset des Targets (falls leer, wird die Start-Lokalposition genutzt)")]
    [SerializeField] private Vector3 defaultTargetPosition = new Vector3(0f, 1.5f, 0f);

    [Tooltip("Versatz des Targets beim Zielen (z. B. X = -0.8 für Schulteransicht, Y = 0.2 für etwas höher)")]
    [SerializeField] private Vector3 aimOffset = new Vector3(-0.8f, 0.2f, 0f);
    
    [Tooltip("Geschwindigkeit, mit der das Target in die Ziel-Position gleitet")]
    [SerializeField] private float transitionSpeed = 10f;

    [Header("Prioritäten")]
    [SerializeField] private int activePriority = 15;
    [SerializeField] private int inactivePriority = 10;

    private bool isAiming = false;
    private Vector3 baseLocalPosition;

    private void Awake()
    {
        if (cameraTarget != null)
        {
            // Nutze die im Editor eingestellte lokale Position als Basis
            baseLocalPosition = cameraTarget.localPosition;

            // Falls sie auf Zero steht, nutze den Ausweichwert aus dem Inspector
            if (baseLocalPosition == Vector3.zero)
            {
                baseLocalPosition = defaultTargetPosition;
                cameraTarget.localPosition = baseLocalPosition;
            }
        }
    }

    void Start()
    {
        // Kameras beim Start sofort in den richtigen Ausgangszustand bringen
        if (normalCamera != null) normalCamera.Priority = activePriority;
        if (aimCamera != null) aimCamera.Priority = inactivePriority;

        // Position des Targets sofort ohne Lerp auf den Normalzustand setzen
        if (cameraTarget != null)
        {
            cameraTarget.localPosition = baseLocalPosition;
        }
    }

    void Update()
    {
        // Rechte Maustaste gedrückt halten zum Zielen
        if (Input.GetMouseButtonDown(1))
        {
            isAiming = true;
            SetAimState(true);
        }

        if (Input.GetMouseButtonUp(1))
        {
            isAiming = false;
            SetAimState(false);
        }

        // Sanfte Verschiebung des KameraTargets je nach Aim-Zustand
        UpdateTargetPosition();
    }

    private void UpdateTargetPosition()
    {
        if (cameraTarget == null) return;

        // Ziel-Position definieren
        Vector3 targetPosition = isAiming ? (baseLocalPosition + aimOffset) : baseLocalPosition;

        // Sanft zur Ziel-Position bewegen (Lerp)
        cameraTarget.localPosition = Vector3.Lerp(
            cameraTarget.localPosition, 
            targetPosition, 
            Time.deltaTime * transitionSpeed
        );
    }

    private void SetAimState(bool aiming)
    {
        if (aiming)
        {
            if (aimCamera != null) aimCamera.Priority = activePriority;
            if (normalCamera != null) normalCamera.Priority = inactivePriority;
        }
        else
        {
            if (normalCamera != null) normalCamera.Priority = activePriority;
            if (aimCamera != null) aimCamera.Priority = inactivePriority;
        }
    }

    public bool IsAiming()
    {
        return isAiming;
    }
}