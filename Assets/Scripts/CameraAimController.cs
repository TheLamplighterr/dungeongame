using UnityEngine;
using Unity.Cinemachine; // Für neuere Cinemachine-Versionen (Unity 6 / Cinemachine v3)
// Falls du eine ältere Unity-Version nutzt, nimm stattdessen: using Cinemachine;

public class CameraAimController : MonoBehaviour
{
    [Header("Kamera-Referenzen")]
    [SerializeField] private CinemachineCamera normalCamera; // Deine normale VCam
    [SerializeField] private CinemachineCamera aimCamera;   // Deine Aim VCam

    [Header("Prioritäten")]
    [SerializeField] private int activePriority = 15;
    [SerializeField] private int inactivePriority = 10;

    void Start()
    {
        // Sicherstellen, dass wir im Normalzustand starten
        normalCamera.Priority = activePriority;
        aimCamera.Priority = inactivePriority;
    }

    void Update()
    {
        // 0 steht für die linke Maustaste (LMB)
        if (Input.GetMouseButtonDown(0))
        {
            StartAiming();
        }
        
        if (Input.GetMouseButtonUp(0))
        {
            StopAiming();
        }
    }

    private void StartAiming()
    {
        // Indem wir der Aim-Kamera eine höhere Priorität geben,
        // schwenkt Cinemachine automatisch zu ihr rüber.
        aimCamera.Priority = activePriority;
        normalCamera.Priority = inactivePriority;
    }

    private void StopAiming()
    {
        normalCamera.Priority = activePriority;
        aimCamera.Priority = inactivePriority;
    }
}