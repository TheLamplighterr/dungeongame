using UnityEngine;
using Unity.Cinemachine; // Für Unity 6 / Cinemachine v3
// using Cinemachine; // Verwende diese Zeile, falls du eine ältere Unity-Version nutzt

public class PlayerCameraController : MonoBehaviour
{
    [Header("Kameras")]
    [SerializeField] private CinemachineCamera normalCamera;
    [SerializeField] private CinemachineCamera aimCamera;

    [Header("Prioritäten")]
    [SerializeField] private int activePriority = 15;
    [SerializeField] private int inactivePriority = 10;

    private bool isAiming = false;

    void Start()
    {
        // Kameras beim Start initialisieren
        if (normalCamera != null) normalCamera.Priority = activePriority;
        if (aimCamera != null) aimCamera.Priority = inactivePriority;
    }

    void Update()
    {
        // Rechte Maustaste gedrückt halten zum Zielen
        if (Input.GetMouseButtonDown(1)) // 1 = Rechte Maustaste (Standard für Aiming)
        {
            isAiming = true;
            SetAimState(true);
        }

        if (Input.GetMouseButtonUp(1))
        {
            isAiming = false;
            SetAimState(false);
        }
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