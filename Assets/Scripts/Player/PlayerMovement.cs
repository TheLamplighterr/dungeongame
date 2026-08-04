using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Bewegung (Normal)")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8.5f;
    [SerializeField] private float gravity = -9.81f;
    
    [Header("Springen & Doppelsprung")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private int maxJumps = 2; // NEU: 2 für Doppelsprung (3 für Dreifachsprung usw.)
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;

    [Header("VFX (Speed Lines)")]
    [SerializeField] private ParticleSystem speedLinesParticles;

    [HideInInspector] public bool canMove = true; 

    [Header("Rotation (Normal)")]
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Rotation (Beim Zielen)")]
    [SerializeField] private float aimSensitivity = 2f;

    private CharacterController controller;
    private PlayerAttack playerAttack; 
    private Transform mainCameraTransform;
    private Vector3 velocity;

    // NEU: Hält nach, wie viele Sprünge in der Luft noch übrig sind
    private int jumpsRemaining;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerAttack = GetComponent<PlayerAttack>(); 
        
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (speedLinesParticles != null && speedLinesParticles.isPlaying)
        {
            speedLinesParticles.Stop();
        }

        // Zu Beginn die verbleibenden Sprünge initialisieren
        jumpsRemaining = maxJumps;
    }

    void Update()
    {
        // 1. Schwerkraft & Boden-Check
        if (controller.isGrounded)
        {
            if (velocity.y < 0)
            {
                velocity.y = -2f;
            }
            // NEU: Sobald wir auf dem Boden stehen, setzen wir die Sprünge zurück
            jumpsRemaining = maxJumps;
        }

        Vector3 moveDirection = Vector3.zero;
        bool shouldPlayVFX = false;

        // 2. Bewegung & Rotation
        if (canMove)
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

            bool isAiming = playerAttack != null && playerAttack.IsAiming();
            bool isSprinting = Input.GetKey(sprintKey) && !isAiming;
            float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

            if (isSprinting && inputDirection.magnitude >= 0.1f)
            {
                shouldPlayVFX = true;
            }

            if (isAiming)
            {
                // --- ZIELEN-BEWEGUNG ---
                if (mainCameraTransform != null)
                {
                    Vector3 camForward = mainCameraTransform.forward;
                    camForward.y = 0;
                    camForward.Normalize();

                    Vector3 camRight = mainCameraTransform.right;
                    camRight.y = 0;
                    camRight.Normalize();

                    moveDirection = (camForward * vertical + camRight * horizontal).normalized;
                    controller.Move(moveDirection * walkSpeed * Time.deltaTime);
                }
            }
            else
            {
                // --- NORMALE BEWEGUNG ---
                if (inputDirection.magnitude >= 0.1f && mainCameraTransform != null)
                {
                    Vector3 camForward = mainCameraTransform.forward;
                    camForward.y = 0;
                    camForward.Normalize();

                    Vector3 camRight = mainCameraTransform.right;
                    camRight.y = 0;
                    camRight.Normalize();

                    moveDirection = camForward * inputDirection.z + camRight * inputDirection.x;
                    controller.Move(moveDirection * currentSpeed * Time.deltaTime);
                }
            }

            // --- SPRINGEN (ANGEPASST FÜR DOPPELSPRUNG) ---
            if (Input.GetKeyDown(jumpKey) && jumpsRemaining > 0)
            {
                // Setzt die Vertikalgeschwindigkeit komplett zurück, damit der zweite Sprung 
                // in der Luft den gleichen "Punch" hat wie der erste (egal wie schnell man fällt)
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                
                // Einen Sprung abziehen
                jumpsRemaining--;
            }

            // Rotation steuern
            HandleRotation(moveDirection);
        }

        HandleSpeedLinesVFX(shouldPlayVFX);

        // 3. Schwerkraft anwenden
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleSpeedLinesVFX(bool play)
    {
        if (speedLinesParticles == null) return;

        if (play)
        {
            if (!speedLinesParticles.isPlaying) speedLinesParticles.Play();
        }
        else
        {
            if (speedLinesParticles.isPlaying) speedLinesParticles.Stop();
        }
    }

    private void HandleRotation(Vector3 moveDirection)
    {
        bool isAiming = playerAttack != null && playerAttack.IsAiming();

        if (isAiming)
        {
            float mouseX = Input.GetAxis("Mouse X") * aimSensitivity;
            transform.Rotate(Vector3.up * mouseX);

            if (mainCameraTransform != null)
            {
                Vector3 cameraForward = mainCameraTransform.forward;
                cameraForward.y = 0f; 
                if (cameraForward.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.LookRotation(cameraForward);
                }
            }
        }
        else
        {
            if (moveDirection.magnitude >= 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, 
                    targetRotation, 
                    Time.deltaTime * rotationSpeed
                );
            }
        }
    }
}