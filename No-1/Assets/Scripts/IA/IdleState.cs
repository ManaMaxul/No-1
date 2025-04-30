using UnityEngine;

public class IdleState : IState
{
    Hunter _hunter;

    public IdleState(Hunter hunter)
    {
        _hunter = hunter;
    }

    public void OnEnter()
    {
        Debug.Log("Hunter: Entering Idle");
    }

    public void OnUpdate()
    {
        _hunter.RecoverEnergy(_hunter.EnergyRecoverRate);
    }

    public void OnExit()
    {
        Debug.Log("Hunter: Exiting Idle");
    }
}