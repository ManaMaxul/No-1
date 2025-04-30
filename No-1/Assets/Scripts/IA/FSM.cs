using System.Collections.Generic;
using UnityEngine;

public class FSM<T>
{
    Dictionary<T, IState> _states = new();
    IState _currentState;

    public void AddState(T newState, IState state)
    {
        if (_states.ContainsKey(newState)) return;
        _states.Add(newState, state);
    }

    public void Execute()
    {
        if (_currentState != null)
            _currentState.OnUpdate();
    }

    public void ChangeState(T newState)
    {
        if (!_states.ContainsKey(newState)) return;
        if (_currentState == _states[newState]) return;

        if (_currentState != null)
            _currentState.OnExit();

        _currentState = _states[newState];
        _currentState.OnEnter();
    }
}