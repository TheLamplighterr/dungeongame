using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public Transform cam;

    public float speed = 6f;
    public float sprintSpeed = 10f;

    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    Vector3 velocity;
    bool isGrounded;

    // Player can move or not (used for inventory UI)
    public bool canMove = true;

    void Update()
    {
        if (!canMove)
            return;

        HandleGround();
        HandleMovement();
        HandleJumpAndGravity();
    }

    void HandleGround()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        float currentSpeed = Input.GetKey(KeyCode.LeftShift)
            ? sprintSpeed
            : speed;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle =
                Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg
                + cam.eulerAngles.y;

            float angle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetAngle,
                ref turnSmoothVelocity,
                turnSmoothTime
            );

            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir =
                Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            controller.Move(moveDir * currentSpeed * Time.deltaTime);
        }
    }

    void HandleJumpAndGravity()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Enemy"))
        {
            Vector3 pushDir = hit.moveDirection;
            pushDir.y = 0;
            controller.Move(-pushDir * 0.05f);
        }
    }
}