using UnityEngine;
using Unity.Cinemachine; // Für neuere Cinemachine-Versionen (Unity 6 / Cinemachine v3)
// Falls ältere Unity-Version: using Cinemachine;

public class PlayerAimController : MonoBehaviour
{
    [Header("Kamera-Referenzen")]
    [SerializeField] private CinemachineCamera normalCamera; // Deine normale VCam
    [SerializeField] private CinemachineCamera aimCamera;   // Deine Aim VCam

    [Header("Prioritäten")]
    [SerializeField] private int activePriority = 15;
    [SerializeField] private int inactivePriority = 10;

    [Header("Zielen & Rotation")]
    [SerializeField] private float aimRotationSpeed = 15f; // Wie schnell dreht sich der Spieler?
    
    private Transform mainCameraTransform;
    private bool isAiming = false;

    void Start()
    {
        // Kamera-Referenz holen (spart uns das ständige Suchen in Update)
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }

        // Standard-Kamera aktivieren
        normalCamera.Priority = activePriority;
        aimCamera.Priority = inactivePriority;
    }

    void Update()
    {
        // 0 = Linke Maustaste gedrückt halten
        if (Input.GetMouseButtonDown(0))
        {
            StartAiming();
        }
        
        if (Input.GetMouseButtonUp(0))
        {
            StopAiming();
        }

        // Wenn wir zielen, richten wir den Spieler aus
        if (isAiming)
        {
            RotatePlayerToCameraDirection();
        }
    }

    private void StartAiming()
    {
        isAiming = true;
        aimCamera.Priority = activePriority;
        normalCamera.Priority = inactivePriority;
    }

    private void StopAiming()
    {
        isAiming = false;
        normalCamera.Priority = activePriority;
        aimCamera.Priority = inactivePriority;
    }

    private void RotatePlayerToCameraDirection()
    {
        if (mainCameraTransform == null) return;

        // 1. Wir holen uns die Vorwärts-Richtung der Kamera
        Vector3 cameraForward = mainCameraTransform.forward;

        // 2. WICHTIG: Wir setzen den Y-Wert auf 0. 
        // Ohne das würde sich dein Spieler nach vorne/hinten neigen, wenn du nach oben/unten schaust!
        cameraForward.y = 0; 

        // Sicherstellen, dass der Vektor noch gültig ist (nicht komplett nach unten geschaut wird)
        if (cameraForward.sqrMagnitude > 0.001f)
        {
            // 3. Ziel-Rotation berechnen
            Quaternion targetRotation = Quaternion.LookRotation(cameraForward);

            // 4. Den Spieler weich (Slerp) in diese Richtung drehen
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, aimRotationSpeed * Time.deltaTime);
        }
    }
}