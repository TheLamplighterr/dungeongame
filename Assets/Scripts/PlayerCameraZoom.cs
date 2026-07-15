using UnityEngine;
using Unity.Cinemachine; // Wichtig für Cinemachine v3

public class PlayerCameraZoom : MonoBehaviour
{
    [Header("Kamera-Referenz")]
    [SerializeField] private CinemachineCamera normalCamera;

    [Header("Zoom-Einstellungen")]
    [SerializeField] private float minDistance = 2f;      
    [SerializeField] private float maxDistance = 10f;     
    [SerializeField] private float zoomSensitivity = 3f;  
    [SerializeField] private float smoothSpeed = 8f;      

    private Cinemachine3rdPersonFollow thirdPersonFollow;
    private CinemachineOrbitalFollow orbitalFollow; // Falls du ein Orbital-Follow benutzt!
    
    private float targetDistance;
    private bool isOrbital = false;

    void Start()
    {
        if (normalCamera == null)
        {
            Debug.LogError("DIAGNOSE: Keine 'normalCamera' im Inspector zugewiesen! Bitte ziehe deine normale Cinemachine-Kamera in den Slot.");
            return;
        }

        // 1. Versuche 3rd Person Follow zu finden
        thirdPersonFollow = normalCamera.GetComponent<Cinemachine3rdPersonFollow>();
        if (thirdPersonFollow != null)
        {
            targetDistance = thirdPersonFollow.CameraDistance;
            Debug.Log($"DIAGNOSE: Erfolg! Cinemachine3rdPersonFollow gefunden. Start-Distanz ist: {targetDistance}");
            return;
        }

        // 2. Versuche Orbital Follow zu finden (sehr häufig in v3!)
        orbitalFollow = normalCamera.GetComponent<CinemachineOrbitalFollow>();
        if (orbitalFollow != null)
        {
            isOrbital = true;
            targetDistance = orbitalFollow.Radius; // Bei Orbital heißt die Distanz Radius
            Debug.Log($"DIAGNOSE: Erfolg! CinemachineOrbitalFollow gefunden. Start-Radius ist: {targetDistance}");
            return;
        }

        Debug.LogError("DIAGNOSE: Weder 'Cinemachine3rdPersonFollow' noch 'CinemachineOrbitalFollow' auf deiner Kamera gefunden! Schau mal im Inspector deiner Kamera, welche Follow-Komponente aktiv ist.");
    }

    void Update()
    {
        // 1. Input abfragen (Sowohl Mausrad ALS AUCH Tasten!)
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (Input.GetKey(KeyCode.O)) 
        {
            scrollInput = -0.1f; 
        }
        else if (Input.GetKey(KeyCode.I)) 
        {
            scrollInput = 0.1f;
        }

        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            // Berechne neue Wunsch-Distanz
            targetDistance -= scrollInput * zoomSensitivity * 10f;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        }

        // 2. Zoom weich anwenden, je nachdem welche Komponente aktiv ist
        if (thirdPersonFollow != null)
        {
            thirdPersonFollow.CameraDistance = Mathf.Lerp(
                thirdPersonFollow.CameraDistance, 
                targetDistance, 
                Time.deltaTime * smoothSpeed
            );
        }
        else if (isOrbital && orbitalFollow != null)
        {
            orbitalFollow.Radius = Mathf.Lerp(
                orbitalFollow.Radius, 
                targetDistance, 
                Time.deltaTime * smoothSpeed
            );
        }
    }
}