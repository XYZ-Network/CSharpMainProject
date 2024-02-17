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
        Vector2Int nextTargetPosition = base.GetNextStep();

        if (nextTargetPosition == unit.Pos)
        {
            if (_lastState == EUnitState.Moving)
                _modeHasChanged = true;
            
            _lastState = EUnitState.Attacking;
        }
        else
        {
            if (_lastState == EUnitState.Attacking)
                _modeHasChanged = true;
            
            _lastState = EUnitState.Moving;
        }
        
        return _modeHasChanged ? unit.Pos : nextTargetPosition;
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
        
        base.Update(deltaTime, time);
    }
}
