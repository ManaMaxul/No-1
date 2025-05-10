using UnityEngine;

public class PlayerMovement : MonoBehaviour, IMovable
{
    private Rigidbody rb;
    private Transform bodyTransform;
    private PlayerEntity playerEntity;
    private Entity entity;
    private Camera camaraJugador;
    private Vector3 currentVelocity;
    private Vector3 targetVelocity;
    private float lastDashTime;
    private bool isDashing;
    private float dashTimeLeft;

    void Awake()
    {
        bodyTransform = transform.Find("Body");
        if (bodyTransform == null)
        {
            Debug.LogError("No se encontró el objeto hijo 'Body'.");
            return;
        }

        rb = bodyTransform.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("No se encontró el componente Rigidbody en el objeto 'Body'.");
            return;
        }

        entity = GetComponent<Entity>();
        playerEntity = GetComponent<PlayerEntity>();
        if (entity == null || playerEntity == null)
        {
            Debug.LogError("No se encontró el componente Entity o PlayerEntity en el jugador.");
            return;
        }
        camaraJugador = Camera.main;

        // Configurar Rigidbody
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        if (isDashing)
        {
            dashTimeLeft -= Time.deltaTime;
            if (dashTimeLeft <= 0)
            {
                EndDash();
            }
            return;
        }

        HandleMovementInput();
        HandleDashInput();
        RotarHaciaMouse();
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            HandleDashMovement();
        }
        else
        {
            HandleNormalMovement();
        }
    }

    private void HandleMovementInput()
    {
        float movimientoHorizontal = Input.GetAxisRaw("Horizontal");
        float movimientoVertical = Input.GetAxisRaw("Vertical");
        Vector3 direccionCamara = new Vector3(movimientoHorizontal, 0f, movimientoVertical).normalized;
        targetVelocity = direccionCamara * playerEntity.velocidadMovimiento;

        if (targetVelocity.magnitude > 0.1f)
        {
            entity.CurrentState = Entity.EntityState.Moving;
        }
        else
        {
            entity.CurrentState = Entity.EntityState.Idle;
        }
    }

    private void HandleDashInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= lastDashTime + playerEntity.dashCooldown)
        {
            PerformDash();
        }
    }

    private void HandleNormalMovement()
    {
        // Suavizar la velocidad actual hacia la velocidad objetivo
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, playerEntity.acceleration * Time.fixedDeltaTime);
        
        // Aplicar la velocidad al Rigidbody
        Vector3 newVelocity = new Vector3(currentVelocity.x, rb.velocity.y, currentVelocity.z);
        rb.velocity = newVelocity;
    }

    private void HandleDashMovement()
    {
        Vector3 dashDirection = bodyTransform.forward * playerEntity.dashForce * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + dashDirection);
    }

    private void PerformDash()
    {
        isDashing = true;
        dashTimeLeft = playerEntity.dashDuration;
        lastDashTime = Time.time;
        entity.CurrentState = Entity.EntityState.Dashing;
        Debug.Log("¡Dash realizado!");
    }

    private void EndDash()
    {
        isDashing = false;
        rb.velocity = Vector3.zero;
        Debug.Log("Dash terminado");
    }

    private void RotarHaciaMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, bodyTransform.position);
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 point = ray.GetPoint(rayDistance);
            Vector3 lookDir = point - bodyTransform.position;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.01f)
            {
                bodyTransform.forward = lookDir.normalized;
            }
        }
    }

    public void Move(Vector3 movimiento)
    {
        if (!isDashing)
        {
            targetVelocity = movimiento;
        }
    }

    void OnDrawGizmos()
    {
        if (playerEntity == null || bodyTransform == null) return;
        if (!playerEntity.mostrarGizmos) return;

        Gizmos.color = playerEntity.colorGizmo;
        Vector3 posicionInicial = bodyTransform.position + Vector3.up * 0.1f;
        Vector3 direccion = bodyTransform.forward * playerEntity.longitudGizmo;
        Gizmos.DrawRay(posicionInicial, direccion);
        Gizmos.DrawSphere(posicionInicial + direccion, 0.1f);
    }
}