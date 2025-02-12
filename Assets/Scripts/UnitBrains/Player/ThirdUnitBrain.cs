using System;
using System.Collections;
using System.Collections.Generic;
using Model;
using UnitBrains.Player;
using UnityEngine;

public class ThirdUnitBrain : DefaultPlayerUnitBrain
{

    public override string TargetUnitName => "Ironclad Behemoth";
    private float _lastStopTime = 0;
    private static readonly int _awaitingTime = 1;
    private enum State
    {
        move,
        attack
    }
    private State _state = State.move;

    public override Vector2Int GetNextStep()
    {
        if (IsMustAwait())
        {
            return unit.Pos;
        }
        var nextStep = base.GetNextStep();
        if (nextStep != unit.Pos && _state == State.attack)
        {
            _state = State.move;
            StopToOneMinute();
            return unit.Pos;
        }
        return nextStep;
    }

    protected override List<Vector2Int> SelectTargets()
    {
        if (IsMustAwait())
        {
            return new List<Vector2Int>();
        }
        var targets = base.SelectTargets();
        if (targets.Count > 0 && _state != State.attack)
        {
            _state = State.attack;
            StopToOneMinute();
            return new List<Vector2Int>();
        }
        return targets;
    }

    private void StopToOneMinute()
    {
        var currentTime = Time.time;
        _lastStopTime = currentTime;
    }

    private bool IsMustAwait()
    {
        var currentTime = Time.time;
        if (currentTime < (_lastStopTime + ThirdUnitBrain._awaitingTime))
        {
            return true;
        }
        return false;
    }
}
