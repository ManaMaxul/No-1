using UnityEngine;

public class HuntingState : IState
{
    Hunter _hunter;
    const float CAPTURE_DISTANCE = 0.5f;

    public HuntingState(Hunter hunter)
    {
        _hunter = hunter;
    }

    public void OnEnter()
    {
        Debug.Log("Hunter: Entering Hunting");
        _hunter.ResetVelocity();
    }

    public void OnUpdate()
    {
        // Drenar energía mientras caza (más rápido que en Patrol)
        _hunter.DrainEnergy(_hunter.EnergyDrainRate * 2f);

        // Verificar si el boid objetivo sigue existiendo
        if (_hunter.TargetBoid == null)
        {
            Debug.Log("Hunter: Target boid lost, transitioning to Patrol");
            _hunter.ChangeState(TypeFSM.Patrol);
            return;
        }

        // Calcular la distancia al boid
        float distanceToBoid = Vector3.Distance(_hunter.transform.position, _hunter.TargetBoid.transform.position);

        // Verificar si el boid está fuera del rango de detección
        if (distanceToBoid > _hunter.DetectionRange)
        {
            Debug.Log($"Hunter: Boid out of range (distance: {distanceToBoid}, detection range: {_hunter.DetectionRange}), transitioning to Patrol");
            _hunter.TargetBoid = null;
            _hunter.ChangeState(TypeFSM.Patrol);
            return;
        }

        // Verificar si el cazador está lo suficientemente cerca para "atrapar" al boid
        if (distanceToBoid < CAPTURE_DISTANCE)
        {
            // Eliminar el boid de la simulación
            GameManager.Instance.boids.Remove(_hunter.TargetBoid);
            Object.Destroy(_hunter.TargetBoid.gameObject);
            Debug.Log($"Hunter: Captured a boid at position {_hunter.TargetBoid.transform.position}, energy remaining: {_hunter.Energy}");
            _hunter.TargetBoid = null;
            _hunter.ChangeState(TypeFSM.Patrol);
            return;
        }

        // Continuar persiguiendo al boid
        _hunter.AddForce(_hunter.Pursuit(_hunter.TargetBoid));
    }

    public void OnExit()
    {
        Debug.Log("Hunter: Exiting Hunting");
    }
}