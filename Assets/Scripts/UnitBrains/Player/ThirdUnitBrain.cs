using Model;
using System.Collections;
using System.Collections.Generic;
using UnitBrains.Player;
using UnityEngine;
using Utilities;

public class ThirdUnitBrain : DefaultPlayerUnitBrain
{
    public enum UnitState
    {
        Move,
        Attack
    }

    public override string TargetUnitName => "Ironclad Behemoth";
    private UnitState _currentUnitState = UnitState.Move;
    private float _timer = 0f;
    private float _timeToChangeState = 0.1f;
    private bool _isStateChange;

    public override Vector2Int GetNextStep()
    {
        Vector2Int target = base.GetNextStep();

        if (target == unit.Pos)
        {
            if (_currentUnitState == UnitState.Move)
            {
                _isStateChange = true;
            }

            _currentUnitState = UnitState.Attack;
        } else
        {
            if (_currentUnitState == UnitState.Attack)
            {
                _isStateChange = true;
            }

            _currentUnitState = UnitState.Move;
        }

        return _isStateChange ? unit.Pos : target;

    }

    protected override List<Vector2Int> SelectTargets()
    {
        if (_isStateChange)
        {
            return new List<Vector2Int>();
        }

        if (_currentUnitState == UnitState.Attack) 
        {
            return base.SelectTargets();
        }

        return new List<Vector2Int>();
    }

    public override void Update(float deltaTime, float time)
    {
        if (_isStateChange)
        {
            _timer += Time.deltaTime;

            if (_timer >= _timeToChangeState)
            {
                _timer = 0f;
                _isStateChange = false;
            }
        }

        base.Update(deltaTime, time);
    }
}
