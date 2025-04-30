using UnityEngine;
using System.Collections.Generic;

public class Hunter : MonoBehaviour
{
    [SerializeField] float maxEnergy = 100f;
    [SerializeField] float energyDrainRate = 10f;
    [SerializeField] float energyRecoverRate = 20f;
    [SerializeField] float detectionRange = 10f;
    [SerializeField] List<Transform> waypoints;
    [SerializeField] float waypointThreshold = 1f;
    [SerializeField] float maxVelocity = 5f;
    [SerializeField] float maxForce = 0.5f;
    [SerializeField] float restDuration = 5f;

    public float MaxEnergy => maxEnergy;
    public float EnergyDrainRate => energyDrainRate;
    public float EnergyRecoverRate => energyRecoverRate;
    public float DetectionRange => detectionRange;
    public float RestDuration => restDuration;

    FSM<TypeFSM> _fsm;
    float _energy;
    Vector3 _velocity;
    int _currentWaypointIndex;
    Character _targetBoid;

    private void Awake()
    {
        _energy = maxEnergy * 0.5f; // Comenzar con 50% de energía para forzar un descanso inicial
        _fsm = new FSM<TypeFSM>();
        _fsm.AddState(TypeFSM.Idle, new RestState(this));
        _fsm.AddState(TypeFSM.Patrol, new PatrolState(this));
        _fsm.AddState(TypeFSM.Hunting, new HuntingState(this));
        _fsm.ChangeState(TypeFSM.Idle);
    }

    private void Update()
    {
        _fsm.Execute();
        transform.position += _velocity * Time.deltaTime;
        if (_velocity != Vector3.zero)
            transform.forward = _velocity;
    }

    public void ChangeState(TypeFSM newState)
    {
        _fsm.ChangeState(newState);
    }

    public void AddForce(Vector3 dir)
    {
        _velocity = Vector3.ClampMagnitude(_velocity + dir * Time.deltaTime, maxVelocity);
    }

    public Vector3 Seek(Vector3 target)
    {
        var desired = target - transform.position;
        desired.Normalize();
        desired *= maxVelocity;
        var steering = desired - _velocity;
        return Vector3.ClampMagnitude(steering, maxForce);
    }

    public Vector3 Pursuit(Character target)
    {
        var desired = target.transform.position + target.Velocity;
        return Seek(desired);
    }

    public void DrainEnergy(float amount)
    {
        _energy = Mathf.Max(0, _energy - amount * Time.deltaTime);
        if (_energy <= 0)
        {
            Debug.Log("Hunter: Energy depleted, transitioning to Rest");
            _fsm.ChangeState(TypeFSM.Idle);
        }
    }

    public void RecoverEnergy(float amount)
    {
        _energy = Mathf.Min(maxEnergy, _energy + amount * Time.deltaTime);
    }

    public Character FindNearestBoid()
    {
        Character nearest = null;
        float minDistance = detectionRange;
        foreach (var boid in GameManager.Instance.boids)
        {
            if (boid == null) continue;
            float distance = Vector3.Distance(transform.position, boid.transform.position);
            if (distance < minDistance)
            {
                nearest = boid;
                minDistance = distance;
            }
        }
        return nearest;
    }

    public Transform GetNextWaypoint()
    {
        if (waypoints.Count == 0) return null;
        return waypoints[_currentWaypointIndex];
    }

    public void MoveToNextWaypoint()
    {
        _currentWaypointIndex = (_currentWaypointIndex + 1) % waypoints.Count;
    }

    public bool IsAtWaypoint(Transform waypoint)
    {
        return Vector3.Distance(transform.position, waypoint.position) < waypointThreshold;
    }

    public void ResetVelocity()
    {
        _velocity = Vector3.zero;
    }

    public void FindNearestWaypoint()
    {
        if (waypoints.Count == 0) return;

        float minDistance = float.MaxValue;
        int nearestIndex = 0;

        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] == null) continue;
            float distance = Vector3.Distance(transform.position, waypoints[i].position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestIndex = i;
            }
        }

        _currentWaypointIndex = nearestIndex;
    }

    public Character TargetBoid
    {
        get => _targetBoid;
        set => _targetBoid = value;
    }

    public float Energy => _energy;

    public void ResetWaypointIndex()
    {
        _currentWaypointIndex = 0;
    }
}

public enum TypeFSM
{
    Idle,
    Patrol,
    Hunting
}