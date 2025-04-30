using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Clase principal que controla el comportamiento del cazador en el juego.
/// Implementa un sistema de máquina de estados finitos (FSM) para manejar diferentes comportamientos:
/// - Descanso (Idle)
/// - Patrullaje (Patrol)
/// - Caza (Hunting)
/// </summary>
public class Hunter : MonoBehaviour
{
    [SerializeField] float maxEnergy = 100f;
    [SerializeField] float energyDrainRate = 10f;
    [SerializeField] float energyRecoverRate = 20f;
    [SerializeField] float detectionRange = 10f;
    [SerializeField] List<Transform> waypoints;
    [SerializeField] float waypointThreshold = 1f;
    [SerializeField] float maxVelocity = 5f;
    [SerializeField] float maxForce = 1f; // Aumentado de 0.5f a 1f para una mejor corrección de trayectoria
    [SerializeField] float restDuration = 5f;

    public float MaxEnergy => maxEnergy;
    public float EnergyDrainRate => energyDrainRate;
    public float EnergyRecoverRate => energyRecoverRate;
    public float DetectionRange => detectionRange;
    public float RestDuration => restDuration;
    public float MaxVelocity => maxVelocity;
    public float MaxForce => maxForce;
    public Vector3 Velocity => _velocity;

    FSM<TypeFSM> _fsm;
    float _energy;
    Vector3 _velocity;
    int _currentWaypointIndex;
    Character _targetBoid;

    /// <summary>
    /// Inicializa el cazador con energía media y configura la máquina de estados.
    /// </summary>
    private void Awake()
    {
        _energy = maxEnergy * 0.5f;
        _fsm = new FSM<TypeFSM>();
        _fsm.AddState(TypeFSM.Idle, new RestState(this));
        _fsm.AddState(TypeFSM.Patrol, new PatrolState(this));
        _fsm.AddState(TypeFSM.Hunting, new HuntingState(this));
        _fsm.ChangeState(TypeFSM.Idle);
    }

    /// <summary>
    /// Actualiza el estado del cazador y su posición en cada frame.
    /// </summary>
    private void Update()
    {
        _fsm.Execute();
        transform.position += _velocity * Time.deltaTime;
        if (_velocity != Vector3.zero)
            transform.forward = _velocity;
    }

    /// <summary>
    /// Cambia el estado actual del cazador a un nuevo estado.
    /// </summary>
    public void ChangeState(TypeFSM newState)
    {
        _fsm.ChangeState(newState);
    }

    /// <summary>
    /// Aplica una fuerza al cazador, limitada por la velocidad máxima.
    /// </summary>
    public void AddForce(Vector3 dir)
    {
        _velocity = Vector3.ClampMagnitude(_velocity + dir, maxVelocity);
    }

    /// <summary>
    /// Calcula la fuerza necesaria para moverse hacia un objetivo específico.
    /// </summary>
    public Vector3 Seek(Vector3 target)
    {
        var desired = target - transform.position;
        desired.Normalize();
        desired *= maxVelocity;
        var steering = desired - _velocity;
        return Vector3.ClampMagnitude(steering, maxForce);
    }

    /// <summary>
    /// Calcula la fuerza necesaria para perseguir a un objetivo en movimiento.
    /// </summary>
    public Vector3 Pursuit(Character target)
    {
        var desired = target.transform.position + target.Velocity;
        return Seek(desired);
    }

    /// <summary>
    /// Reduce la energía del cazador. Si llega a cero, cambia al estado de descanso.
    /// </summary>
    public void DrainEnergy(float amount)
    {
        _energy = Mathf.Max(0, _energy - amount * Time.deltaTime);
        if (_energy <= 0)
        {
            Debug.Log("Hunter: Energy depleted, transitioning to Rest");
            _fsm.ChangeState(TypeFSM.Idle);
        }
    }

    /// <summary>
    /// Recupera energía del cazador hasta el máximo permitido.
    /// </summary>
    public void RecoverEnergy(float amount)
    {
        _energy = Mathf.Min(maxEnergy, _energy + amount * Time.deltaTime);
    }

    /// <summary>
    /// Encuentra el boid más cercano dentro del rango de detección.
    /// </summary>
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

    /// <summary>
    /// Obtiene el siguiente waypoint en la ruta de patrullaje.
    /// </summary>
    public Transform GetNextWaypoint()
    {
        if (waypoints.Count == 0) return null;
        return waypoints[_currentWaypointIndex];
    }

    /// <summary>
    /// Avanza al siguiente waypoint en la secuencia.
    /// </summary>
    public void MoveToNextWaypoint()
    {
        _currentWaypointIndex = (_currentWaypointIndex + 1) % waypoints.Count;
    }

    /// <summary>
    /// Verifica si el cazador ha llegado a un waypoint específico.
    /// </summary>
    public bool IsAtWaypoint(Transform waypoint)
    {
        return Vector3.Distance(transform.position, waypoint.position) < waypointThreshold;
    }

    /// <summary>
    /// Reinicia la velocidad del cazador a cero.
    /// </summary>
    public void ResetVelocity()
    {
        _velocity = Vector3.zero;
    }

    /// <summary>
    /// Encuentra el waypoint más cercano a la posición actual del cazador.
    /// </summary>
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

    /// <summary>
    /// Dibuja gizmos para visualizar el rango de detección, waypoints y línea al objetivo.
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (waypoints != null)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < waypoints.Count; i++)
            {
                if (waypoints[i] == null) continue;
                Gizmos.DrawWireSphere(waypoints[i].position, 0.5f);
                if (i < waypoints.Count - 1 && waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                }
            }
        }

        if (_targetBoid != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, _targetBoid.transform.position);
        }
    }
}

/// <summary>
/// Enum que define los posibles estados del cazador en la máquina de estados finitos.
/// </summary>
public enum TypeFSM
{
    Idle,    // Estado de descanso
    Patrol,  // Estado de patrullaje
    Hunting  // Estado de caza
}