using Model.Runtime.Projectiles;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Player;
using UnityEngine;
using Utilities;

enum ThirdUnitMode
{
    Move,
    Attack,
    SwitchMode
}

public class ThirdUnitBrain : DefaultPlayerUnitBrain
{
    public override string TargetUnitName => "Ironclad Behemoth";
    const float ModeChangeDelay = 1f;
    private float _modeChangeTimer = ModeChangeDelay;
    private ThirdUnitMode _currentMode = ThirdUnitMode.Move;
    private float _modeSwitchDeadline;

    public override Vector2Int GetNextStep()
    {
        switch (_currentMode)
        {
            case ThirdUnitMode.Move:
                return base.GetNextStep();
            case ThirdUnitMode.Attack:
            case ThirdUnitMode.SwitchMode:
                return unit.Pos;
            default:
                return unit.Pos;
        }
    }

    protected override List<Vector2Int> SelectTargets()
    {
        switch (_currentMode)
        {
            case ThirdUnitMode.Move:
                return new List<Vector2Int>();
            case ThirdUnitMode.Attack:
                return base.SelectTargets();
            case ThirdUnitMode.SwitchMode:
                return new List<Vector2Int>();
            default:
                return new List<Vector2Int>();
        }
    }

    public override void Update(float deltaTime, float time)
    {
        bool hasTargets = HasTargetsInRange();
        
        if (_currentMode == ThirdUnitMode.SwitchMode)
        {
            if (time < _modeSwitchDeadline)
            {
                base.Update(deltaTime, time);
                return;
            }

            if (hasTargets)
            {
                _currentMode = ThirdUnitMode.Attack;
            }
            else
            {
                _currentMode = ThirdUnitMode.Move;
            }
        }
        else if (hasTargets && _currentMode == ThirdUnitMode.Move)
        {
            StartSwitchMode(time);
        }
        else if (!hasTargets && _currentMode == ThirdUnitMode.Attack)
        {
            StartSwitchMode(time);
        }
        base.Update(deltaTime, time);
    }
    private void StartSwitchMode(float time)
    {
        _currentMode = ThirdUnitMode.SwitchMode;
        _modeSwitchDeadline = time + ModeChangeDelay;
    }
}
