using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] float _maxVelocity = 3f;
    [SerializeField] float _maxForce = 0.3f;
    [SerializeField] float _radiusSeparation = 2f;
    [SerializeField] float _radiusDetection = 5f;
    [SerializeField] float _foodDetectionRange = 10f;
    [SerializeField] float _hunterDetectionRange = 8f;

    Vector3 _velocity;
    public Vector3 Velocity => _velocity;

    // Variables para persistencia de comportamiento
    enum Behavior { Flocking, SeekFood, EvadeHunter, Random }
    Behavior _currentBehavior;
    float _behaviorTimer = 0f;
    const float BEHAVIOR_DURATION = 1f;

    // Variables para movimiento aleatorio suave
    Vector3 _randomTargetDirection;
    float _randomDirectionTimer = 0f;
    const float RANDOM_DIRECTION_CHANGE_INTERVAL = 2f;

    private void Start()
    {
        GameManager.Instance.boids.Add(this);
        _velocity = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * _maxVelocity;
        _currentBehavior = Behavior.Random;
        _randomTargetDirection = _velocity.normalized;
    }

    private void Update()
    {
        _behaviorTimer -= Time.deltaTime;
        _randomDirectionTimer -= Time.deltaTime;

        if (_behaviorTimer <= 0)
        {
            DecideBehavior();
            _behaviorTimer = BEHAVIOR_DURATION;
        }

        ExecuteBehavior();

        transform.position += _velocity * Time.deltaTime;
        if (_velocity != Vector3.zero)
            transform.forward = _velocity;
    }

    void DecideBehavior()
    {
        var food = FindNearestFood();
        var hunter = GameManager.Instance.Hunter; // Usar la propiedad pública Hunter

        if (food != null)
        {
            _currentBehavior = Behavior.SeekFood;
        }
        else if (hunter != null && Vector3.Distance(transform.position, hunter.position) < _hunterDetectionRange)
        {
            _currentBehavior = Behavior.EvadeHunter;
        }
        else if (HasNearbyBoids())
        {
            _currentBehavior = Behavior.Flocking;
        }
        else
        {
            _currentBehavior = Behavior.Random;
        }
    }

    void ExecuteBehavior()
    {
        switch (_currentBehavior)
        {
            case Behavior.SeekFood:
                var food = FindNearestFood();
                if (food != null)
                {
                    AddForce(Arrive(food.position));
                    if (Vector3.Distance(transform.position, food.position) < 0.5f)
                    {
                        GameManager.Instance.foods.Remove(food);
                        Destroy(food.gameObject);
                        _behaviorTimer = 0;
                    }
                }
                else
                {
                    _behaviorTimer = 0;
                }
                break;

            case Behavior.EvadeHunter:
                var hunter = GameManager.Instance.Hunter; // Usar la propiedad pública Hunter
                if (hunter != null && Vector3.Distance(transform.position, hunter.position) < _hunterDetectionRange)
                {
                    AddForce(Evade(hunter));
                }
                else
                {
                    _behaviorTimer = 0;
                }
                break;

            case Behavior.Flocking:
                if (HasNearbyBoids())
                {
                    Flocking();
                }
                else
                {
                    _behaviorTimer = 0;
                }
                break;

            case Behavior.Random:
                AddForce(RandomMovement());
                break;
        }
    }

    void Flocking()
    {
        AddForce(Separation(GameManager.Instance.boids, _radiusSeparation) * GameManager.Instance.WeightSeparation); // Usar WeightSeparation
        AddForce(Alignment(GameManager.Instance.boids, _radiusDetection) * GameManager.Instance.WeightAlignment); // Usar WeightAlignment
        AddForce(Cohesion(GameManager.Instance.boids, _radiusDetection) * GameManager.Instance.WeightCohesion); // Usar WeightCohesion
    }

    Vector3 Separation(List<Character> boids, float radius)
    {
        Vector3 desired = Vector3.zero;
        foreach (var item in boids)
        {
            if (item == null || item == this) continue;
            var dir = transform.position - item.transform.position;
            if (dir.magnitude > radius) continue;
            desired += dir.normalized / (dir.magnitude + 0.01f);
        }
        if (desired == Vector3.zero) return desired;
        desired.Normalize();
        desired *= _maxVelocity;
        var steering = desired - _velocity;
        return Vector3.ClampMagnitude(steering, _maxForce);
    }

    Vector3 Alignment(List<Character> boids, float radius)
    {
        Vector3 desired = Vector3.zero;
        int count = 0;
        foreach (var item in boids)
        {
            if (item == null || item == this) continue;
            var dir = transform.position - item.transform.position;
            if (dir.magnitude > radius) continue;
            desired += item.Velocity;
            count++;
        }
        if (count == 0) return Vector3.zero;
        desired /= count;
        desired.Normalize();
        desired *= _maxVelocity;
        var steering = desired - _velocity;
        return Vector3.ClampMagnitude(steering, _maxForce);
    }

    Vector3 Cohesion(List<Character> boids, float radius)
    {
        Vector3 desired = Vector3.zero;
        int count = 0;
        foreach (var item in boids)
        {
            if (item == null || item == this) continue;
            var dir = transform.position - item.transform.position;
            if (dir.magnitude > radius) continue;
            desired += item.transform.position;
            count++;
        }
        if (count == 0) return Vector3.zero;
        desired /= count;
        desired = desired - transform.position;
        desired.Normalize();
        desired *= _maxVelocity;
        var steering = desired - _velocity;
        return Vector3.ClampMagnitude(steering, _maxForce);
    }

    Vector3 Arrive(Vector3 target)
    {
        var desired = target - transform.position;
        float dist = desired.magnitude;
        desired.Normalize();
        if (dist < 2f)
            desired *= _maxVelocity * (dist / 2f);
        else
            desired *= _maxVelocity;
        var steering = desired - _velocity;
        return Vector3.ClampMagnitude(steering, _maxForce);
    }

    Vector3 Evade(Transform target)
    {
        var hunter = target.GetComponent<Character>();
        var futurePos = target.position + (hunter ? hunter.Velocity : Vector3.zero);
        var desired = transform.position - futurePos;
        desired.Normalize();
        desired *= _maxVelocity;
        desired = Vector3.Lerp(_velocity.normalized * _maxVelocity, desired, 0.5f);
        var steering = desired - _velocity;
        return Vector3.ClampMagnitude(steering, _maxForce);
    }

    Vector3 RandomMovement()
    {
        if (_randomDirectionTimer <= 0)
        {
            _randomTargetDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
            _randomDirectionTimer = RANDOM_DIRECTION_CHANGE_INTERVAL;
        }

        var desired = _randomTargetDirection * _maxVelocity;
        var steering = desired - _velocity;
        return Vector3.ClampMagnitude(steering, _maxForce);
    }

    Transform FindNearestFood()
    {
        Transform nearest = null;
        float minDistance = _foodDetectionRange;
        foreach (var food in GameManager.Instance.foods)
        {
            if (food == null) continue;
            float distance = Vector3.Distance(transform.position, food.position);
            if (distance < minDistance)
            {
                nearest = food;
                minDistance = distance;
            }
        }
        return nearest;
    }

    bool HasNearbyBoids()
    {
        foreach (var boid in GameManager.Instance.boids)
        {
            if (boid == null || boid == this) continue;
            if (Vector3.Distance(transform.position, boid.transform.position) < _radiusDetection)
                return true;
        }
        return false;
    }

    public void AddForce(Vector3 dir)
    {
        _velocity = Vector3.ClampMagnitude(_velocity + dir, _maxVelocity);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _radiusSeparation);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _radiusDetection);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _foodDetectionRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _hunterDetectionRange);
    }
}