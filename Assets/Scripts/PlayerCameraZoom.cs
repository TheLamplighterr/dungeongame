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
    private CinemachineOrbitalFollow orbitalFollow;
    
    private float targetDistance;
    private bool isOrbital = false;

    void Start()
    {
        if (normalCamera == null)
        {
            Debug.LogError("DIAGNOSE: Keine 'normalCamera' im Inspector zugewiesen!");
            return;
        }

        // 1. Versuche 3rd Person Follow zu finden
        thirdPersonFollow = normalCamera.GetComponent<Cinemachine3rdPersonFollow>();
        if (thirdPersonFollow != null)
        {
            targetDistance = thirdPersonFollow.CameraDistance;
            return;
        }

        // 2. Versuche Orbital Follow zu finden
        orbitalFollow = normalCamera.GetComponent<CinemachineOrbitalFollow>();
        if (orbitalFollow != null)
        {
            isOrbital = true;
            targetDistance = orbitalFollow.Radius;
            return;
        }
    }

    void Update()
    {
        // Wenn das Spiel pausiert ist (z.B. Inventar offen), keinen Zoom verarbeiten
        if (Time.timeScale == 0f) return;

        // 1. Nur das Mausrad abfragen! (KeyCode.I wurde entfernt, da es das Inventar triggert)
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            // Berechne neue Wunsch-Distanz
            targetDistance -= scrollInput * zoomSensitivity * 10f;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        }

        // 2. Zoom weich anwenden
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