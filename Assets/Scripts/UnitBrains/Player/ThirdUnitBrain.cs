using System.Collections.Generic;
using UnitBrains.Player;
using UnityEngine;

public class ThirdUnitBrain : DefaultPlayerUnitBrain
{
    private const float _changingTime = 0.1f;

    public override string TargetUnitName => "Ironclad Behemoth";

    private bool _isRunMode = true;
    private bool _isAttackMode = false;
    private bool _isChangingMode = false;
    private float _timeBeforeChange = 0f;

    public float TimeBeforeChange
    {
        get => _timeBeforeChange;
        set
        {
            if(_timeBeforeChange != value)
            {
                if (!_isChangingMode)
                {
                    _isChangingMode = true;
                    _timeBeforeChange = value;
                    return;
                }

                if(_timeBeforeChange < 0)
                {
                    _isChangingMode = false;
                    _timeBeforeChange = 0f;

                    if (_isAttackMode)
                    {
                        _isRunMode = true;
                        _isAttackMode = false;
                    }
                    else if (_isRunMode)
                    {
                        _isRunMode = false;
                        _isAttackMode = true;
                    }
                }
                else
                {
                    _timeBeforeChange = value;
                }
            }
        }
    }

    public override void Update(float deltaTime, float time)
    {
        base.Update(deltaTime, time);

        if (_isChangingMode)
        {
            TimeBeforeChange -= deltaTime;
        }
    }

    public override Vector2Int GetNextStep()
    {
        var nextStep = base.GetNextStep();

        if (_isChangingMode)
        {
            return unit.Pos;
        }

        if (!_isRunMode && nextStep != unit.Pos)
        {
            TimeBeforeChange = _changingTime;
        }

        if (_isRunMode)
        {
            return nextStep;
        }

        return unit.Pos;
    }

    protected override List<Vector2Int> SelectTargets()
    {
        var targets =  base.SelectTargets();

        if (_isChangingMode)
        {
            return new List<Vector2Int>();
        }

        if (targets.Count > 0 && !_isAttackMode)
        {
            TimeBeforeChange = _changingTime;
        }

        if (_isAttackMode)
        {
            return targets;
        }

        return new List<Vector2Int>();
    }
}
