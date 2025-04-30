using UnityEngine;

public class RestState : IState
{
    Hunter _hunter;
    float _restTimer;
    bool _hasRested; // Para asegurar que el cazador descanse al menos una vez al inicio

    public RestState(Hunter hunter)
    {
        _hunter = hunter;
    }

    public void OnEnter()
    {
        Debug.Log("Hunter: Entering Rest");
        _hunter.ResetVelocity();
        _restTimer = _hunter.RestDuration;
        _hasRested = false;
    }

    public void OnUpdate()
    {
        _hunter.RecoverEnergy(_hunter.EnergyRecoverRate);
        _restTimer -= Time.deltaTime;

        // Solo permitir salir de Rest si ha pasado el tiempo de descanso y la energía está llena
        if (_restTimer <= 0)
        {
            _hasRested = true;
        }

        if (_hasRested && _hunter.Energy >= _hunter.MaxEnergy)
        {
            Debug.Log($"Hunter: Energy fully recovered ({_hunter.Energy}/{_hunter.MaxEnergy}), transitioning to Patrol");
            _hunter.ChangeState(TypeFSM.Patrol);
        }
    }

    public void OnExit()
    {
        Debug.Log("Hunter: Exiting Rest");
    }
}