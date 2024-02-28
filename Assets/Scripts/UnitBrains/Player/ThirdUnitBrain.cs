using System;using System.Collections.Generic;
using UnitBrains.Player;
using UnityEngine;

public enum EUnitState
{
    Moving,
    Attacking,
}

public class ThirdUnitBrain : DefaultPlayerUnitBrain
{
    public override string TargetUnitName => "Ironclad Behemoth";
    private EUnitState _lastState = EUnitState.Moving;
    private bool _modeHasChanged;
    private float _switchingTime = 0.1f;
    private float _timer;
    
    public override Vector2Int GetNextStep()
    {
        return _modeHasChanged ? unit.Pos : base.GetNextStep();
    }

    protected override List<Vector2Int> SelectTargets()
    {
        if (_modeHasChanged)
            return new List<Vector2Int>();

        if (_lastState == EUnitState.Attacking)
            return base.SelectTargets();
        
        return new List<Vector2Int>();
    }
    
    public override void Update(float deltaTime, float time)
    {
        if (_modeHasChanged)
        {
            _timer += Time.deltaTime;

            if (_timer >= _switchingTime)
            {
                _timer = 0f;
                _modeHasChanged = false;
            }
        }
        
        ChangeUnitState();
        base.Update(deltaTime, time);
    }
    
    private void ChangeUnitState()
    {
        Vector2Int nextTargetPosition = base.GetNextStep();

        if (nextTargetPosition == unit.Pos)
        {
            CheckModForChanges(EUnitState.Attacking);
            _lastState = EUnitState.Attacking;
        }
        else
        {
            CheckModForChanges(EUnitState.Moving);
            _lastState = EUnitState.Moving;
        }
    }
    
    private void CheckModForChanges(EUnitState newState)
    {
        if (_lastState != newState)
            _modeHasChanged = true;
    }
    
    
}
