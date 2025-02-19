using System.Collections;
using System.Collections.Generic;
using Model;
using UnitBrains.Pathfinding;
using UnitBrains.Player;
using UnityEngine;

public class ThirdUnitBrain : DefaultPlayerUnitBrain
{
    public override string TargetUnitName => "Ironclad Behemoth";

    private enum UnitState
    {
        Move,
        Attack
    }
   
    private UnitState _currentState = UnitState.Move;
    private bool _isSwitching = false;
    private float _switchingTime = 0f;
    private const float _switchingDuration = 1f;

    public override void Update(float deltaTime, float time)
    {
        if (_isSwitching)
        {
            if ((_switchingTime + _switchingDuration) < time)
            {
                _isSwitching = false;
                _currentState = _currentState == UnitState.Move ? UnitState.Attack : UnitState.Move;
            }
        }
        else 
        {
            bool hasTargetsInRange = HasTargetsInRange();

            if (hasTargetsInRange && _currentState == UnitState.Move
            || !hasTargetsInRange && _currentState == UnitState.Attack)
            {
                _isSwitching = true;
                _switchingTime = time;
            }
        }
    }

    public override Vector2Int GetNextStep()
    {
        if (_currentState == UnitState.Attack)
            return unit.Pos;

        return base.GetNextStep();
    }

    protected override List<Vector2Int> SelectTargets()
    {
        if (_currentState == UnitState.Move)
            return new List<Vector2Int>();
        
        return base.SelectTargets();
    }
}
