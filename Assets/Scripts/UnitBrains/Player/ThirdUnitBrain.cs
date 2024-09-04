using System.Collections;
using System.Collections.Generic;
using UnitBrains.Player;
using UnityEditor.Graphs;
using UnityEngine;

enum UnitState
{
    Move,
    Shoot
}

public class ThirdUnitBrain : DefaultPlayerUnitBrain
{
    public override string TargetUnitName => "Ironclad Behemoth";
    private UnitState _state = UnitState.Move;
    private bool _isChangingState;
    private float _state—hangeTime = .1f;

    public override void Update(float deltaTime, float time)
    {
        if (_isChangingState)
        {
            _state—hangeTime -= deltaTime;
            if (_state—hangeTime <= 0f)
            {
                _isChangingState = false;
                _state—hangeTime = .1f;
            }
        }
        ChangeUnitType();
        base.Update(deltaTime, time);
    }

    public override Vector2Int GetNextStep()
    {
       return _isChangingState ? unit.Pos : base.GetNextStep();
    }

    protected override List<Vector2Int> SelectTargets()
    {  
        if (_isChangingState)
            return new List<Vector2Int>();
        if (_state == UnitState.Shoot)
            return base.SelectTargets();
        return new List<Vector2Int>();
    }

    private void checkCurrentUnitState(UnitState state)
    {
        _isChangingState = state != _state ? true : false;
    }

    private void ChangeUnitType()
    {
        var currentPosition = base.GetNextStep();
        if (currentPosition == unit.Pos)
        {
            checkCurrentUnitState(UnitState.Shoot);
            _state = UnitState.Shoot;
        }
        else
        {
            checkCurrentUnitState(UnitState.Move);
            _state = UnitState.Move;
        }
    }
}
