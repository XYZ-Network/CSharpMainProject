using System.Collections;
using System.Collections.Generic;
using UnitBrains.Player;
using UnityEngine;
public enum UnitStatus
{
    Attacking,
    Moving
}

public class ThirdUnitBrain : DefaultPlayerUnitBrain
{
    public override string TargetUnitName => "Ironclad Behemoth";
    private bool _switchMode;
    private float _timer = 0f;
    private float _switchTimer = 0.1f; // это 1 сек?
    private UnitStatus _status = UnitStatus.Moving; // начинаем с Moving, чтобы в начале не было лишней задержки 1 сек

    public override Vector2Int GetNextStep()
    {
        Vector2Int _nextTargetPos = base.GetNextStep();
        if (_nextTargetPos == unit.Pos)
        {
            if (_status == UnitStatus.Moving)
            {
                _switchMode = true;
            }
            _status = UnitStatus.Attacking;
        } else
        {
            if (_status == UnitStatus.Attacking)
            {
                _switchMode = true;
            }                
            _status = UnitStatus.Moving;
        }

        if (_switchMode)
        {
            return unit.Pos;
        } else
        {
            return _nextTargetPos;
        }
    }

    protected override List<Vector2Int> SelectTargets()
    {
        if (_switchMode)
        {
            return new List<Vector2Int>();
        }
        if (_status == UnitStatus.Attacking)
        {
            return base.SelectTargets();
        }
        return new List<Vector2Int>();
    }

    // Update is called once per frame
    public override void Update(float deltaTime, float time)
    {
        if (_switchMode)
        {
            _timer += Time.deltaTime;
            if (_timer >= _switchTimer)
            {
                _timer = 0f;
                _switchMode = false;
            }
        }
    }
}
