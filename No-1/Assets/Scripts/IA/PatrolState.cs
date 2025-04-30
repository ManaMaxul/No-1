using UnityEngine;

public class PatrolState : IState
{
    Hunter _hunter;
    float _patrolTimer; // Temporizador para tiempo mínimo en Patrol
    const float MIN_PATROL_TIME = 2f; // Tiempo mínimo en Patrol antes de buscar boids (2 segundos)

    public PatrolState(Hunter hunter)
    {
        _hunter = hunter;
    }

    public void OnEnter()
    {
        Debug.Log("Hunter: Entering Patrol");
        _hunter.ResetVelocity();
        _hunter.ResetWaypointIndex();
        _hunter.FindNearestWaypoint();
        _patrolTimer = MIN_PATROL_TIME;
    }

    public void OnUpdate()
    {
        _hunter.DrainEnergy(_hunter.EnergyDrainRate);

        // Reducir el temporizador
        _patrolTimer -= Time.deltaTime;

        // Solo buscar boids después de haber patrullado por un tiempo mínimo
        if (_patrolTimer <= 0)
        {
            var boid = _hunter.FindNearestBoid();
            if (boid != null)
            {
                Debug.Log($"Hunter: Detected a boid at distance {Vector3.Distance(_hunter.transform.position, boid.transform.position)}, transitioning to Hunting");
                _hunter.TargetBoid = boid;
                _hunter.ChangeState(TypeFSM.Hunting);
                return;
            }
        }

        var waypoint = _hunter.GetNextWaypoint();
        if (waypoint == null) return;

        _hunter.AddForce(_hunter.Seek(waypoint.position));
        if (_hunter.IsAtWaypoint(waypoint))
            _hunter.MoveToNextWaypoint();
    }

    


    public void OnExit()
    {
        Debug.Log("Hunter: Exiting Patrol");
    }
}