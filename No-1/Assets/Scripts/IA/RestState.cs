using UnityEngine;

/// <summary>
/// Estado de descanso del cazador.
/// Controla la recuperación de energía y el tiempo de descanso.
/// Implementa la lógica de transición al estado de patrullaje cuando la energía está recuperada.
/// </summary>
public class RestState : IState
{
    Hunter _hunter;
    float _restTimer;
    bool _hasRested; // Controla que el cazador complete al menos un ciclo de descanso

    public RestState(Hunter hunter)
    {
        _hunter = hunter;
    }

    /// <summary>
    /// Se ejecuta al entrar en el estado de descanso.
    /// Inicializa el temporizador y detiene el movimiento.
    /// </summary>
    public void OnEnter()
    {
        Debug.Log("Hunter: Entering Rest");
        _hunter.ResetVelocity();
        _restTimer = _hunter.RestDuration;
        _hasRested = false;
    }

    /// <summary>
    /// Actualiza el estado de descanso en cada frame.
    /// Maneja la recuperación de energía y las condiciones para cambiar de estado.
    /// </summary>
    public void OnUpdate()
    {
        // Recupera energía durante el descanso
        _hunter.RecoverEnergy(_hunter.EnergyRecoverRate);
        _restTimer -= Time.deltaTime;

        // Marca que se ha completado el tiempo de descanso
        if (_restTimer <= 0)
        {
            _hasRested = true;
        }

        // Cambia al estado de patrullaje cuando la energía está recuperada
        if (_hasRested && _hunter.Energy >= _hunter.MaxEnergy)
        {
            Debug.Log($"Hunter: Energy fully recovered ({_hunter.Energy}/{_hunter.MaxEnergy}), transitioning to Patrol");
            _hunter.ChangeState(TypeFSM.Patrol);
        }
    }

    /// <summary>
    /// Se ejecuta al salir del estado de descanso.
    /// </summary>
    public void OnExit()
    {
        Debug.Log("Hunter: Exiting Rest");
    }
}