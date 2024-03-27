using System;
using System.Collections.Generic;
using UnitBrains.Player;
using UnityEngine;

public enum UnitState
{
    Move,
    Attack
}

public class ThirdUnitBrain : DefaultPlayerUnitBrain
{
    public override string TargetUnitName => "Ironclad Behemoth";

    private float _timer = 1f;
    private UnitState _unitState = UnitState.Move;
    private bool _isStateChange = true;

    public override Vector2Int GetNextStep()
    {
        Vector2Int position = base.GetNextStep();

        if (position == unit.Pos)
        {
            if (_unitState == UnitState.Move)
            {
                _isStateChange = true;
            }

            _unitState = UnitState.Attack;
        }
        else
        {
            if (_unitState == UnitState.Attack)
            {
                _isStateChange = true;
            }
            _unitState = UnitState.Move;

        }
        return base.GetNextStep();
    }

    public override void Update(float deltaTime, float time)
    {
        if (_isStateChange)
        {
            _timer -= Time.deltaTime * 10;

            if (_timer <= 0)
            {
                Console.Clear();
                _timer = 1f;
                _isStateChange = false;
            }
        }
        base.Update(deltaTime, time);
    }

    protected override List<Vector2Int> SelectTargets()
    {
        if (_isStateChange)
            return new List<Vector2Int>();

        if(_unitState == UnitState.Attack)
            return base.SelectTargets();

        return new List<Vector2Int>();
    }
}
