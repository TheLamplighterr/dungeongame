using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Bewegung (Normal)")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8.5f; // NEU: Sprintgeschwindigkeit
    [SerializeField] private float gravity = -9.81f;
    
    [Header("Springen")]
    [SerializeField] private float jumpHeight = 1.5f; // NEU: Sprunghöhe
    [SerializeField] private KeyCode jumpKey = KeyCode.Space; // NEU: Sprungtaste
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift; // NEU: Sprinttaste

    [Header("VFX (Speed Lines)")]
    [SerializeField] private ParticleSystem speedLinesParticles; // NEU: Hier das Partikelsystem im Inspector reinziehen

    // Vom Inventar-Skript gesucht: Bestimmt, ob sich der Spieler bewegen darf
    [HideInInspector] public bool canMove = true; 

    [Header("Rotation (Normal)")]
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Rotation (Beim Zielen)")]
    [SerializeField] private float aimSensitivity = 2f; // Maus-Sensibilität beim Zielen

    private CharacterController controller;
    private PlayerAttack playerAttack; 
    private Transform mainCameraTransform;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerAttack = GetComponent<PlayerAttack>(); 
        
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }

        // Sperrt den Mauszeiger im Spielfenster
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Sicherheits-Check: Falls das Partikelsystem beim Start läuft, stoppen wir es kurz
        if (speedLinesParticles != null && speedLinesParticles.isPlaying)
        {
            speedLinesParticles.Stop();
        }
    }

    void Update()
    {
        // 1. Schwerkraft-Vorbereitung (läuft immer)
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        Vector3 moveDirection = Vector3.zero;
        bool shouldPlayVFX = false; // NEU: Merkt sich, ob wir den EFFEKT zeigen wollen

        // 2. Bewegung & Rotation nur ausführen, wenn wir uns bewegen dürfen!
        if (canMove)
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

            // Fragt direkt die IsAiming()-Funktion aus PlayerAttack ab!
            bool isAiming = playerAttack != null && playerAttack.IsAiming();

            // SPRINTEN: Sprinten ist nur aktiv, wenn wir Shift drücken und NICHT zielen
            bool isSprinting = Input.GetKey(sprintKey) && !isAiming;
            float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

            // NEU: Effekt nur aktivieren, wenn wir sprinten UND uns auch wirklich bewegen!
            if (isSprinting && inputDirection.magnitude >= 0.1f)
            {
                shouldPlayVFX = true;
                
                // TEST-MELDUNG: Gibt eine Nachricht in deiner Unity-Konsole aus
                Debug.Log("Sprintschnittstelle AKTIV: Partikel sollten starten!");
            }

            if (isAiming)
            {
                // --- GENSHIN AIM MODE MOVEMENT (STRAFING) ---
                if (mainCameraTransform != null)
                {
                    Vector3 camForward = mainCameraTransform.forward;
                    camForward.y = 0;
                    camForward.Normalize();

                    Vector3 camRight = mainCameraTransform.right;
                    camRight.y = 0;
                    camRight.Normalize();

                    moveDirection = (camForward * vertical + camRight * horizontal).normalized;
                    
                    // Der Charakter bewegt sich starr in diese Richtung (immer im normalen Gehtempo für präzises Zielen)
                    controller.Move(moveDirection * walkSpeed * Time.deltaTime);
                }
            }
            else
            {
                // --- NORMALER BEWEGUNGS-MODUS ---
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

            // --- SPRINGEN (NEU) ---
            // Nur springen, wenn wir auf dem Boden stehen und die Taste drücken
            if (Input.GetKeyDown(jumpKey) && controller.isGrounded)
            {
                // Physikalische Formel für Sprunghöhe: v = sqrt(h * -2 * g)
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            // Rotation steuern
            HandleRotation(moveDirection);
        }

        // NEU: Partikeleffekt basierend auf der Bewegung steuern (mit Null-Check, um Fehler zu vermeiden)
        HandleSpeedLinesVFX(shouldPlayVFX);

        // 3. Schwerkraft physikalisch anwenden
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // NEU: Eigene Methode zur sauberen Steuerung des Partikelsystems
    private void HandleSpeedLinesVFX(bool play)
    {
        if (speedLinesParticles == null) return; // Abbrechen, falls kein Partikelsystem zugewiesen wurde

        if (play)
        {
            if (!speedLinesParticles.isPlaying)
            {
                speedLinesParticles.Play();
            }
        }
        else
        {
            if (speedLinesParticles.isPlaying)
            {
                speedLinesParticles.Stop();
            }
        }
    }

    private void HandleRotation(Vector3 moveDirection)
    {
        bool isAiming = playerAttack != null && playerAttack.IsAiming();

        if (isAiming)
        {
            // --- ZIELEN-ROTATION (GENSHIN STYLE) ---
            // Die Maus dreht den Spieler-Körper direkt um die Y-Achse (links/rechts).
            float mouseX = Input.GetAxis("Mouse X") * aimSensitivity;
            transform.Rotate(Vector3.up * mouseX);

            // Sicherheits-Check: Absolut synchron mit dem Kamera-Forward-Vektor schauen
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
            // --- NORMALE ROTATION ---
            // Wenn wir uns bewegen, drehen wir uns weich in die Laufrichtung
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