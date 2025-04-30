using UnityEngine;

/// <summary>
/// Estado de caza del cazador.
/// Controla el comportamiento de persecución y captura de boids.
/// Implementa la lógica de seguimiento y captura de presas.
/// </summary>
public class HuntingState : IState
{
    Hunter _hunter;
    const float CAPTURE_DISTANCE = 1f;

    public HuntingState(Hunter hunter)
    {
        _hunter = hunter;
    }

    /// <summary>
    /// Se ejecuta al entrar en el estado de caza.
    /// Reinicia la velocidad del cazador.
    /// </summary>
    public void OnEnter()
    {
        Debug.Log("Hunter: Entering Hunting");
        _hunter.ResetVelocity();
    }

    /// <summary>
    /// Actualiza el estado de caza en cada frame.
    /// Maneja la lógica de persecución, captura y transiciones de estado.
    /// </summary>
    public void OnUpdate()
    {
        // Drena energía durante la caza
        _hunter.DrainEnergy(_hunter.EnergyDrainRate);

        // Verifica si se perdió el objetivo
        if (_hunter.TargetBoid == null)
        {
            Debug.Log("Hunter: Target boid lost, transitioning to Patrol");
            _hunter.TargetBoid = null;
            _hunter.ChangeState(TypeFSM.Patrol);
            return;
        }

        // Calcula la distancia al objetivo
        float distanceToBoid = Vector3.Distance(_hunter.transform.position, _hunter.TargetBoid.transform.position);
        Debug.Log($"Hunter: Distance to boid: {distanceToBoid}, Hunter velocity: {_hunter.Velocity.magnitude}, Boid velocity: {_hunter.TargetBoid.Velocity.magnitude}");

        // Verifica si el objetivo está fuera del rango de detección
        if (distanceToBoid > _hunter.DetectionRange)
        {
            Debug.Log($"Hunter: Boid out of range (distance: {distanceToBoid} > detection range: {_hunter.DetectionRange}), transitioning to Patrol");
            _hunter.TargetBoid = null;
            _hunter.ChangeState(TypeFSM.Patrol);
            return;
        }

        // Verifica si se puede capturar al objetivo
        if (distanceToBoid < CAPTURE_DISTANCE)
        {
            if (GameManager.Instance.boids.Contains(_hunter.TargetBoid))
            {
                GameManager.Instance.boids.Remove(_hunter.TargetBoid);
                if (_hunter.TargetBoid.gameObject != null)
                {
                    Object.Destroy(_hunter.TargetBoid.gameObject);
                }
                Debug.Log($"Hunter: Captured a boid at position: {_hunter.TargetBoid.transform.position}, energy remaining: {_hunter.Energy}");
            }
            else
            {
                Debug.Log("Hunter: Target boid was already removed from GameManager.boids");
            }
            _hunter.TargetBoid = null;
            _hunter.ChangeState(TypeFSM.Patrol);
            return;
        }

        // Calcula la dirección de persecución
        Vector3 desired = _hunter.TargetBoid.transform.position - _hunter.transform.position;
        desired.Normalize();

        // Ajusta la velocidad según la distancia al objetivo
        if (distanceToBoid < 2f)
        {
            // Reduce la velocidad cuando está cerca del objetivo
            desired *= _hunter.MaxVelocity * (distanceToBoid / 2f + 0.5f);
        }
        else
        {
            // Mantiene la velocidad máxima cuando está lejos
            desired *= _hunter.MaxVelocity;
        }

        // Ajusta la fuerza de dirección según la distancia
        float tempMaxForce = distanceToBoid < 2f ? _hunter.MaxForce * 2f : _hunter.MaxForce;
        var steering = desired - _hunter.Velocity;
        steering = Vector3.ClampMagnitude(steering, tempMaxForce);

        // Aplica la fuerza calculada
        _hunter.AddForce(steering);
    }

    /// <summary>
    /// Se ejecuta al salir del estado de caza.
    /// </summary>
    public void OnExit()
    {
        Debug.Log("Hunter: Exiting Hunting");
    }
}