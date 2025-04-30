using UnityEngine;

/// <summary>
/// Estado de patrullaje del cazador.
/// Controla el comportamiento de movimiento entre waypoints y la detección de presas.
/// Implementa la lógica de navegación y transición a otros estados.
/// </summary>
public class PatrolState : IState
{
    Hunter _hunter;
    float _patrolTimer;
    const float MIN_PATROL_TIME = 2f;

    public PatrolState(Hunter hunter)
    {
        _hunter = hunter;
    }

    /// <summary>
    /// Se ejecuta al entrar en el estado de patrullaje.
    /// Inicializa el temporizador y encuentra el waypoint más cercano.
    /// </summary>
    public void OnEnter()
    {
        Debug.Log("Hunter: Entering Patrol");
        _hunter.ResetVelocity();
        _hunter.FindNearestWaypoint();
        _patrolTimer = MIN_PATROL_TIME;
    }

    /// <summary>
    /// Actualiza el estado de patrullaje en cada frame.
    /// Maneja la lógica de movimiento entre waypoints y la detección de presas.
    /// </summary>
    public void OnUpdate()
    {
        _hunter.DrainEnergy(_hunter.EnergyDrainRate);
        _patrolTimer -= Time.deltaTime;

        var boid = _hunter.FindNearestBoid();
        if (boid != null)
        {
            Debug.Log($"Hunter: Detected a boid at distance {Vector3.Distance(_hunter.transform.position, boid.transform.position)}, transitioning to HUNTING");
            _hunter.TargetBoid = boid;
            _hunter.ChangeState(TypeFSM.Hunting);
            return;
        }

        var waypoint = _hunter.GetNextWaypoint();
        if (waypoint == null)
        {
            Debug.Log("Hunter: No waypoints available, transitioning to Rest");
            _hunter.ChangeState(TypeFSM.Idle);
            return;
        }

        if (_hunter.IsAtWaypoint(waypoint) && _patrolTimer <= 0)
        {
            _hunter.MoveToNextWaypoint();
            _patrolTimer = MIN_PATROL_TIME;
        }

        _hunter.AddForce(_hunter.Seek(waypoint.position));
    }

    /// <summary>
    /// Se ejecuta al salir del estado de patrullaje.
    /// </summary>
    public void OnExit()
    {
        Debug.Log("Hunter: Exiting Patrol");
    }
}