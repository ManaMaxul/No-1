using UnityEngine;
public class Player : Entity
{
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float dashDistance = 5f;
    private float gravity = -9.81f;

    protected override void Awake()
    {
        base.Awake();
        entityType = EntityType.Player; // Set as Player type
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        // Basic movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move.normalized * MoveSpeed * Time.deltaTime); // Use MoveSpeed property

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Dash
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Vector3 dashDirection = move.normalized == Vector3.zero ? transform.forward : move.normalized;
            controller.Move(dashDirection * dashDistance);
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Update state
        CurrentState = move.magnitude > 0 ? EntityState.Moving : EntityState.Idle; // Fixed case: currentState -> CurrentState
    }

    protected override void Die()
    {
        Debug.Log("Player has died!");
        // Add death logic (e.g., respawn, game over)
    }
}