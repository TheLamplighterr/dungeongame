using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Bewegung")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;
    
    // Vom Inventar-Skript gesucht: Bestimmt, ob sich der Spieler bewegen darf
    [HideInInspector] public bool canMove = true; 

    [Header("Rotation (Normal)")]
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Rotation (Beim Zielen)")]
    [SerializeField] private float aimSensitivity = 2f; // Maus-Sensibilität beim Zielen

    private CharacterController controller;
    private PlayerAttack playerAttack; // GEÄNDERT: Nutzt jetzt direkt das Attack-Skript für den Aim-Status!
    private Transform mainCameraTransform;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerAttack = GetComponent<PlayerAttack>(); // GEÄNDERT: Holt sich die PlayerAttack-Komponente
        
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }

        // Sperrt den Mauszeiger im Spielfenster
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 1. Schwerkraft-Vorbereitung (läuft immer)
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        Vector3 moveDirection = Vector3.zero;

        // 2. Bewegung & Rotation nur ausführen, wenn wir uns bewegen dürfen!
        if (canMove)
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

            // GEÄNDERT: Fragt jetzt direkt die IsAiming()-Funktion aus PlayerAttack ab!
            bool isAiming = playerAttack != null && playerAttack.IsAiming();

            if (isAiming)
            {
                // --- GENSHIN AIM MODE MOVEMENT (STRAFING) ---
                if (mainCameraTransform != null)
                {
                    // Wir holen uns die Vorwärts- und Rechts-Vektoren der Kamera
                    Vector3 camForward = mainCameraTransform.forward;
                    camForward.y = 0;
                    camForward.Normalize();

                    Vector3 camRight = mainCameraTransform.right;
                    camRight.y = 0;
                    camRight.Normalize();

                    // Wichtig: Die Bewegung wird relativ zur Kamera aufgeteilt.
                    moveDirection = (camForward * vertical + camRight * horizontal).normalized;
                    
                    // Der Charakter bewegt sich starr in diese Richtung
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
                    controller.Move(moveDirection * walkSpeed * Time.deltaTime);
                }
            }

            // Rotation steuern (wichtig: wird auch aufgerufen, wenn WSAD nicht gedrückt ist!)
            HandleRotation(moveDirection);
        }

        // 3. Schwerkraft physikalisch anwenden
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleRotation(Vector3 moveDirection)
    {
        // GEÄNDERT: Auch hier die Abfrage auf PlayerAttack umgestellt!
        bool isAiming = playerAttack != null && playerAttack.IsAiming();

        if (isAiming)
        {
            // --- ZIELEN-ROTATION (GENSHIN STYLE) ---
            // 1. Die Maus dreht den Spieler-Körper direkt um die Y-Achse (links/rechts).
            float mouseX = Input.GetAxis("Mouse X") * aimSensitivity;
            transform.Rotate(Vector3.up * mouseX);

            // 2. Sicherheits-Check: Wir stellen sicher, dass der Charakter absolut 
            // synchron mit dem Kamera-Forward-Vektor schaut.
            if (mainCameraTransform != null)
            {
                Vector3 cameraForward = mainCameraTransform.forward;
                cameraForward.y = 0f; // Verhindert, dass der Charakter nach oben/unten wegkippt
                if (cameraForward.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.LookRotation(cameraForward);
                }
            }
        }
        else
        {
            // --- NORMALE ROTATION ---
            // Charakter dreht sich weich in die Richtung, in die er läuft (moveDirection)
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