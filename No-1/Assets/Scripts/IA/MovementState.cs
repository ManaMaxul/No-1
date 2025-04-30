using UnityEngine;

public class MovementState : IState
{
    public void OnEnter()
    {
        Debug.Log("OnEnter Movement");
    }

    public void OnUpdate()
    {
        Debug.Log("OnUpdate Movement");
    }

    public void OnExit()
    {
        Debug.Log("OnExit Movement");
    }
}
