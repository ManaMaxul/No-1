using UnityEngine;

/// <summary>
/// Estado de inactividad del cazador.
/// Estado base que permite la recuperación de energía.
/// </summary>
public class IdleState : IState
{
    Hunter _hunter;

    public IdleState(Hunter hunter)
    {
        _hunter = hunter;
    }

    /// <summary>
    /// Se ejecuta al entrar en el estado de inactividad.
    /// </summary>
    public void OnEnter()
    {
        Debug.Log("Hunter: Entering Idle");
    }

    /// <summary>
    /// Actualiza el estado de inactividad en cada frame.
    /// Recupera energía del cazador.
    /// </summary>
    public void OnUpdate()
    {
        _hunter.RecoverEnergy(_hunter.EnergyRecoverRate);
    }

    /// <summary>
    /// Se ejecuta al salir del estado de inactividad.
    /// </summary>
    public void OnExit()
    {
        Debug.Log("Hunter: Exiting Idle");
    }
}