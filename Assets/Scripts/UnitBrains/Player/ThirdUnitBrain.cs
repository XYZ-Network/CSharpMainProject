using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Player;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{

}
public class Third : DefaultPlayerUnitBrain
{
    public override string TargetUnitName => "Ironclad Behemoth";

    private float _transitionStartTime;
    private State _currentState = State.Moving;
    private State _targetState = State.Moving;
    private bool IsTransitionTimePassed => Time.time - _transitionStartTime >= 1.0f;
    private bool IsTransitionInProgress => _currentState == State.Transition;
    enum State
    {
        Moving,
        Attacking,
        Transition
    }

    public override Vector2Int GetNextStep()
    {
        //Debug.Log($"Current state1: {_currentState}");
        //Debug.Log($"Target state1: {_targetState}");
        if (_currentState == State.Moving)
        {
            return base.GetNextStep();
        }
        else
        {
            return unit.Pos;
        }
    }

    protected override List<Vector2Int> SelectTargets()
    {
        //Debug.Log($"Current state2: {_currentState}");
        //Debug.Log($"Target state2: {_targetState}");
        if (_currentState == State.Attacking)
        {
            return base.SelectTargets();
        }
        else
        {
            return new List<Vector2Int>();
        }
    }

    public override void Update(float deltaTime, float time)
    {
        if (IsTransitionInProgress)
        {
            if (IsTransitionTimePassed)
            {
                _currentState = _targetState;
            }
        }
        else
        {
            var haveReacheableTargets = GetReachableTargets().Any();
            if (haveReacheableTargets && _currentState == State.Moving)
            {
                StartTransitionToState(State.Attacking);
            }
            else if (!haveReacheableTargets && _currentState == State.Attacking)
            {
                StartTransitionToState(State.Moving);
            }
        }
    }

    private void StartTransitionToState(State targetState)
    {
        _transitionStartTime = Time.time;
        _currentState = State.Transition;
        _targetState = targetState;
    }
}
