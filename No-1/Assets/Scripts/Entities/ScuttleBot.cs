using UnityEngine;
using System.Collections.Generic;
using UnityEngine.ProBuilder;

public class ScuttleBot : Enemy
{
    [Header("Comportamiento")]
    [SerializeField] private float wanderRadius = 3f;
    [SerializeField] private float wanderTimer = 2f;
    [SerializeField] private float retreatDistance = 5f;
    [SerializeField] private float maxVelocity = 5f;
    [SerializeField] private float maxForce = 0.3f;

    [Header("Ataque")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float projectileDamage = 5f;
    [SerializeField] private float coneAngle = 30f;

    private Vector3 velocity;
    private Vector3 wanderTarget;
    private float currentWanderTimer;
    private bool hasShot;
    private Rigidbody rb;
    private float initialY;
    private Transform playerBody;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        currentWanderTimer = wanderTimer;
        SetNewWanderTarget();
        CurrentEnemyState = EnemyState.Idle;
        initialY = transform.position.y;
        detectionRange = 6f;
        
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        }

        // Buscar el cuerpo físico del jugador
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerBody = playerObj.transform.Find("Body");
            if (playerBody == null)
            {
                playerBody = playerObj.transform;
            }
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.scuttleBots.Add(this);
        }
        else
        {
            Debug.LogWarning("GameManager no encontrado en la escena. El ScuttleBot funcionará sin sistema de oleadas.");
        }

        // Typing y rareza para el ScuttleBot
        typeRarity = new Inventory.TypeRarity();
        typeRarity.types.Add(Inventory.DamageType.Lightning);
        typeRarity.rarity = Inventory.Rarity.TwoStars;
        resistances = new List<Inventory.DamageType> { Inventory.DamageType.Lightning };
        weaknesses = new List<Inventory.DamageType> { Inventory.DamageType.Earth };
    }

    void Update()
    {
        if (playerBody == null || CurrentHealth <= 0) return;

        // Mantener la posición Y constante
        transform.position = new Vector3(transform.position.x, initialY, transform.position.z);

        // Aplicar fuerza de límites
        if (GameManager.Instance != null)
        {
            Vector3 boundaryForce = GameManager.Instance.GetBoundaryForce(transform.position);
            if (boundaryForce != Vector3.zero)
            {
                AddForce(boundaryForce);
            }
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerBody.position);

        if (distanceToPlayer <= detectionRange)
        {
            transform.LookAt(new Vector3(playerBody.position.x, transform.position.y, playerBody.position.z));
            
            if (!hasShot)
            {
                ShootCone();
                hasShot = true;
                SetRetreatTarget();
            }
            else if (CurrentEnemyState == EnemyState.Retreating)
            {
                Wander();
                if (Vector3.Distance(transform.position, wanderTarget) < 0.5f)
                {
                    hasShot = false;
                }
            }
        }
        else
        {
            CurrentEnemyState = EnemyState.Moving;
            Wander();
            hasShot = false;
        }
    }

    private void ShootCone()
    {
        if (projectilePrefab == null || ProjectilePool.Instance == null) return;

        for (int i = -1; i <= 1; i++)
        {
            float angle = i * coneAngle;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
            
            GameObject projectile = ProjectilePool.Instance.GetProjectile();
            if (projectile != null)
            {
                projectile.transform.position = transform.position + transform.forward * 0.5f;
                projectile.transform.rotation = Quaternion.LookRotation(direction);

                Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
                if (projectileRb != null)
                {
                    projectileRb.velocity = direction * projectileSpeed;
                }

                Projectile projectileScript = projectile.GetComponent<Projectile>();
                if (projectileScript != null)
                {
                    projectileScript.SetDamage(projectileDamage);
                }
            }
        }
    }

    private void SetRetreatTarget()
    {
        Vector3 retreatDirection = (transform.position - playerBody.position).normalized;
        wanderTarget = new Vector3(
            transform.position.x + retreatDirection.x * retreatDistance,
            initialY,
            transform.position.z + retreatDirection.z * retreatDistance
        );
        CurrentEnemyState = EnemyState.Retreating;
    }

    private void Wander()
    {
        currentWanderTimer -= Time.deltaTime;

        if (currentWanderTimer <= 0)
        {
            SetNewWanderTarget();
            currentWanderTimer = wanderTimer;
        }

        Vector3 direction = (wanderTarget - transform.position).normalized;
        Vector3 desired = direction * maxVelocity;
        Vector3 steering = desired - velocity;
        steering = Vector3.ClampMagnitude(steering, maxForce);
        AddForce(steering);
    }

    private void SetNewWanderTarget()
    {
        Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
        wanderTarget = new Vector3(
            transform.position.x + randomCircle.x,
            initialY,
            transform.position.z + randomCircle.y
        );
    }

    private void AddForce(Vector3 force)
    {
        Vector3 horizontalForce = new Vector3(force.x, 0, force.z);
        velocity = Vector3.ClampMagnitude(velocity + horizontalForce, maxVelocity);
    }

    void FixedUpdate()
    {
        if (CurrentEnemyState == EnemyState.Moving || CurrentEnemyState == EnemyState.Retreating)
        {
            Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
            rb.velocity = horizontalVelocity;
        }
    }

    protected override void Die()
    {
        CurrentEnemyState = EnemyState.Dead;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.scuttleBots.Remove(this);
            GameManager.Instance.CheckWaveCompletion();
        }
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);
        
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, wanderTarget);
        }
    }
}