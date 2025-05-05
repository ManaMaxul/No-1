using UnityEngine;

public class PlayerMovement : Entity
{
    [Header("Movimiento")]
    public float velocidadMovimiento = 5f;

    [Header("Ataque")]
    public float attackRange = 2f;
    public float attackAngle = 45f;
    public float attackCooldown = 0.5f;
    private float lastAttackTime;

    [Header("Escudo")]
    public float shieldRadius = 2f;
    public float shieldPushForce = 5f;
    public GameObject shieldObject;
    private bool shieldActive;

    [Header("Cámara Fija")]
    public float distanciaCamara = 5f;
    public float alturaCamara = 5f;
    public float anguloCamara = 45f;

    [Header("Debug")]
    public bool mostrarGizmos = true;
    public float longitudGizmo = 2f;
    public Color colorGizmo = Color.blue;

    private Rigidbody rb;
    private Vector3 movimientoActual;
    private Camera camaraJugador;
    private Transform bodyTransform;

    protected override void Awake()
    {
        base.Awake();
        bodyTransform = transform.Find("Body");
        if (bodyTransform == null)
        {
            Debug.LogError("No se encontró el objeto hijo 'Body'. Asegúrate de que existe y tiene el nombre correcto.");
            return;
        }

        rb = bodyTransform.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("No se encontró el componente Rigidbody en el objeto 'Body'.");
            return;
        }

        camaraJugador = Camera.main;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ActualizarCamara();

        if (shieldObject != null)
        {
            shieldObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("shieldObject no está asignado en el Inspector.");
        }
    }

    void Update()
    {
        float movimientoHorizontal = Input.GetAxisRaw("Horizontal");
        float movimientoVertical = Input.GetAxisRaw("Vertical");
        Vector3 direccionCamara = new Vector3(movimientoHorizontal, 0f, movimientoVertical).normalized;
        movimientoActual = direccionCamara * velocidadMovimiento;

        if (movimientoActual.magnitude > 0.1f)
        {
            CurrentState = EntityState.Moving;
        }
        else if (Input.GetMouseButtonDown(0) && CanAttack())
        {
            CurrentState = EntityState.Attacking;
            Attack();
        }
        else
        {
            CurrentState = EntityState.Idle;
        }

        if (CanUseShield())
        {
            if (Input.GetKey(KeyCode.Space) && !shieldActive)
            {
                ActivateShield();
            }
            else if (!Input.GetKey(KeyCode.Space) && shieldActive)
            {
                DeactivateShield();
            }

            if (shieldActive)
            {
                CheckShieldCollisions();
            }
        }

        RotarHaciaMouse();
        ActualizarCamara();
    }

    void FixedUpdate()
    {
        Move(movimientoActual);
        Vector3 rotacionActual = bodyTransform.rotation.eulerAngles;
        bodyTransform.rotation = Quaternion.Euler(0f, rotacionActual.y, 0f);
    }

    void Move(Vector3 movimiento)
    {
        Vector3 nuevaVelocidad = new Vector3(movimiento.x, rb.velocity.y, movimiento.z);
        rb.velocity = nuevaVelocidad;
    }

    void ActualizarCamara()
    {
        Vector3 direccionCamara = new Vector3(0f, alturaCamara, -distanciaCamara);
        Quaternion rotacionCamara = Quaternion.Euler(anguloCamara, 0f, 0f);
        Vector3 posicionDeseada = bodyTransform.position + rotacionCamara * direccionCamara;
        camaraJugador.transform.position = posicionDeseada;
        camaraJugador.transform.rotation = rotacionCamara;
    }

    void RotarHaciaMouse()
    {
        Vector3 posicionMouse = Input.mousePosition;
        Ray rayoMouse = camaraJugador.ScreenPointToRay(posicionMouse);
        Plane planoSuelo = new Plane(Vector3.up, bodyTransform.position);
        float distancia;

        if (planoSuelo.Raycast(rayoMouse, out distancia))
        {
            Vector3 puntoMundo = rayoMouse.GetPoint(distancia);
            Vector3 direccion = (puntoMundo - bodyTransform.position).normalized;
            direccion.y = 0f;

            if (direccion != Vector3.zero)
            {
                Quaternion rotacionDeseada = Quaternion.LookRotation(direccion, Vector3.up);
                bodyTransform.rotation = Quaternion.Slerp(bodyTransform.rotation, rotacionDeseada, Time.deltaTime * 10f);
            }
        }
    }

    bool CanAttack()
    {
        return (inventory.leftHandItem?.type == Inventory.ItemType.Espada || inventory.rightHandItem?.type == Inventory.ItemType.Espada) && Time.time >= lastAttackTime + attackCooldown;
    }

    void Attack()
    {
        lastAttackTime = Time.time;

        Collider[] hits = Physics.OverlapSphere(bodyTransform.position + bodyTransform.forward * attackRange / 2, attackRange / 2);
        foreach (Collider hit in hits)
        {
            Entity enemy = hit.GetComponent<Entity>();
            if (enemy != null && enemy.Type != Type)
            {
                enemy.TakeDamage(PlayerStats.Instance.AttackDamage);
            }
        }

        Debug.Log("Jugador atacó!");
    }

    bool CanUseShield()
    {
        return (inventory.leftHandItem?.type == Inventory.ItemType.Escudo || inventory.rightHandItem?.type == Inventory.ItemType.Escudo);
    }

    void ActivateShield()
    {
        shieldActive = true;
        if (shieldObject != null)
        {
            shieldObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("No se puede activar el escudo: shieldObject es null.");
        }
        Debug.Log("Escudo activado!");
    }

    void DeactivateShield()
    {
        shieldActive = false;
        if (shieldObject != null)
        {
            shieldObject.SetActive(false);
        }
        Debug.Log("Escudo desactivado!");
    }

    void CheckShieldCollisions()
    {
        Collider[] hits = Physics.OverlapSphere(bodyTransform.position, shieldRadius);
        foreach (Collider hit in hits)
        {
            Entity enemy = hit.GetComponent<Entity>();
            if (enemy != null && enemy.Type != Type)
            {
                Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
                if (enemyRb != null)
                {
                    Vector3 pushDirection = (enemy.transform.position - bodyTransform.position).normalized;
                    enemyRb.AddForce(pushDirection * shieldPushForce, ForceMode.Impulse);
                    Debug.Log("Enemigo empujado por el escudo!");
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!mostrarGizmos || bodyTransform == null) return;

        Gizmos.color = colorGizmo;
        Vector3 posicionInicial = bodyTransform.position + Vector3.up * 0.1f; // Elevar ligeramente para mejor visibilidad
        Vector3 direccion = bodyTransform.forward * longitudGizmo;
        Gizmos.DrawRay(posicionInicial, direccion);
        Gizmos.DrawSphere(posicionInicial + direccion, 0.1f);

        if (CurrentState == EntityState.Attacking)
        {
            Gizmos.color = Color.red;
            Vector3 leftDir = Quaternion.Euler(0, -attackAngle / 2, 0) * bodyTransform.forward;
            Vector3 rightDir = Quaternion.Euler(0, attackAngle / 2, 0) * bodyTransform.forward;
            Gizmos.DrawRay(posicionInicial, leftDir * attackRange);
            Gizmos.DrawRay(posicionInicial, rightDir * attackRange);
            Gizmos.DrawRay(posicionInicial, bodyTransform.forward * attackRange);
            int segments = 10;
            for (int i = 0; i <= segments; i++)
            {
                float angle = -attackAngle / 2 + (attackAngle * i / segments);
                Vector3 dir = Quaternion.Euler(0, angle, 0) * bodyTransform.forward;
                Gizmos.DrawRay(posicionInicial, dir * attackRange);
            }
        }

        if (shieldActive)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(bodyTransform.position, shieldRadius);
        }
    }

    protected override void Die()
    {
        Debug.Log("Jugador ha muerto");
    }
}