using UnityEngine;
using System.Collections.Generic;
using UnityEngine.ProBuilder;

public class ScuttleBot : Entity
{
    [Header("Comportamiento")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float wanderRadius = 5f;
    [SerializeField] private float wanderTimer = 2f;
    [SerializeField] private float wanderForce = 2f;

    [Header("Ataque")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float projectileDamage = 5f;
    [SerializeField] private float shootInterval = 2f;
    [SerializeField] private float coneAngle = 30f;

    private Vector3 velocity;
    private Vector3 wanderTarget;
    private float currentWanderTimer;
    private float shootTimer;
    private Transform player;
    private Rigidbody rb;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        currentWanderTimer = wanderTimer;
        SetNewWanderTarget();
        GameManager.Instance.scuttleBots.Add(this);
    }

    void Update()
    {
        if (player == null || CurrentHealth <= 0) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            CurrentState = EntityState.Attacking;
            transform.LookAt(player);
            ShootCone();
            velocity = Vector3.zero;
        }
        else
        {
            CurrentState = EntityState.Moving;
            Wander();
        }
    }

    void FixedUpdate()
    {
        if (CurrentState == EntityState.Moving)
        {
            rb.velocity = velocity;
            if (velocity != Vector3.zero)
            {
                transform.forward = velocity.normalized;
            }
        }
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
        velocity = direction * wanderForce;
    }

    private void SetNewWanderTarget()
    {
        Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
        wanderTarget = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
    }

    private void ShootCone()
    {
        shootTimer += Time.deltaTime;
        if (shootTimer >= shootInterval && projectilePrefab != null)
        {
            for (int i = -1; i <= 1; i++)
            {
                float angle = i * coneAngle;
                Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
                
                GameObject projectile = Instantiate(projectilePrefab, 
                    transform.position + transform.forward * 0.5f, 
                    Quaternion.LookRotation(direction));

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
            shootTimer = 0f;
        }
    }

    protected override void Die()
    {
        GameManager.Instance.scuttleBots.Remove(this);
        GameManager.Instance.CheckWaveCompletion();
        Destroy(gameObject);
    }

    void OnDrawGizmos()
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