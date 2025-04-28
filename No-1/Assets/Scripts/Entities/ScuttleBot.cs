using UnityEngine;
using System.Collections.Generic;

//TPFinal - [Your Name]
public class ScuttleBot : Entity
{
    [SerializeField] private float patrolRange = 5f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 10f;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Transform player;
    private float shootTimer;
    private const float shootInterval = 2f;

    // Dictionary to store patrol points
    private Dictionary<int, Vector3> patrolPoints = new Dictionary<int, Vector3>();

    protected override void Awake()
    {
        base.Awake();
        entityType = EntityType.Enemy; // Set as Enemy type
        startPosition = transform.position;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        SetPatrolPoints();
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            CurrentState = EntityState.Attacking;
            transform.LookAt(player);
            ShootCone();
        }
        else
        {
            CurrentState = EntityState.Moving;
            Patrol();
        }
    }

    private void SetPatrolPoints()
    {
        patrolPoints[0] = startPosition;
        patrolPoints[1] = startPosition + Vector3.right * patrolRange;
    }

    private void Patrol()
    {
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            targetPosition = patrolPoints[Random.Range(0, patrolPoints.Count)];
        }
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, MoveSpeed * Time.deltaTime);
    }

    private void ShootCone()
    {
        shootTimer += Time.deltaTime;
        if (shootTimer >= shootInterval)
        {
            for (int i = -1; i <= 1; i++)
            {
                GameObject projectile = Instantiate(projectilePrefab, transform.position + transform.forward, Quaternion.identity);
                Vector3 direction = (transform.forward + transform.right * i * 0.3f).normalized;
                projectile.GetComponent<Rigidbody>().velocity = direction * projectileSpeed;
            }
            shootTimer = 0f;
        }
    }

    protected override void Die()
    {
        Debug.Log("ScuttleBot destroyed!");
        Destroy(gameObject);
    }
}