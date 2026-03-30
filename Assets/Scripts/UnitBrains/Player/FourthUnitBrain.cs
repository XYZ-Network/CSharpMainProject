using Assets.Scripts.UnitBrains;
using Model.Runtime.ReadOnly;
using System.Collections.Generic;
using UnitBrains.Player;
using UnityEngine;
using Utilities;

public class FourthUnitBrain : DefaultPlayerUnitBrain
{
    public override string TargetUnitName => "Buffalo";
    private bool _cooldown = false;
    private bool _pause = false;
    private float _cooldownLengthSec = 2f;
    private float _pauseLengthSec = 0.5f;
    private float _pauseTimer = 0f;
    private float _cooldownTimer = 0f;
    private IReadOnlyUnit _unitToBuff;
    private BuffManager _buffManager => ServiceLocator.Get<BuffManager>();

    protected override List<Vector2Int> SelectTargets()
    {
        return new List<Vector2Int>();
    }

    public override Vector2Int GetNextStep()
    {
        if (IsPause())
        {
            return unit.Pos;
        }
        return base.GetNextStep();
    }

    private bool IsPause()
    {
        return Time.time - _pauseTimer < _pauseLengthSec;
    }

    public override void Update(float deltaTime, float time)
    {
        ApplyBuff();
        if (!IsPause())
        {
            _pause = false;
        }
        if (!_pause)
        {
            if (Time.time - _cooldownTimer >= _cooldownLengthSec)
            {
                _cooldown = false;
            }
            if (!_cooldown)
            {
                var units = GetReachableOurUnits();
                foreach (IReadOnlyUnit unit in units)
                {
                    if (unit == this.unit)
                    {
                        continue;
                    }
                    if (!_buffManager.existByUnit(unit))
                    {
                        Debug.Log($"Buff to {unit.Config.Name}");
                        _unitToBuff = unit;
                        SetPause();
                        break;
                    }
                }
            }
        }
        base.Update(deltaTime, time);
    }

    private void ApplyBuff()
    {
        if (_unitToBuff == null) return;
        if (!IsPause())
        {
            _buffManager.AddRandomBuff(_unitToBuff);
            SetPause();
            _unitToBuff = null;
            _cooldown = true;
            _cooldownTimer = Time.time;
        }
    }


    private void SetPause()
    {
        _pause = true;
        _pauseTimer = Time.time;
    }
}
