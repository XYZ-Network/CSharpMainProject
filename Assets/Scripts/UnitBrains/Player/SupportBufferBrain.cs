using Assets.Scripts.BuffsDebuffsSystem;
using Model.Runtime;
using Model.Runtime.ReadOnly;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnitBrains;
using UnitBrains.Player;
using UnityEngine;
using Utilities;
using View;

public class SupportBufferBrain : BaseUnitBrain
{
    public override string TargetUnitName => "Support Buffer";

    private bool _isMoving = true;

    private float _buffCooldown = 3;
    private float _castTime = 0.5f;
    private float _afterCastDelay = 0.5f;
    private float _lastCastTime = 0f;
    private float _waitingTime = 0f;

    private EffectManager _effectManager;

    public SupportBufferBrain()
    {
        _effectManager = ServiceLocator.Get<EffectManager>();
    }
    
    public override Vector2Int GetNextStep()
    {
        return _isMoving ? base.GetNextStep() : unit.Pos;
    }

    protected override List<Vector2Int> SelectTargets()
    {
        return new();
    }

    public override void Update(float deltaTime, float time)
    {
        if (IsBuffReady() && IsReadyToCast() && !_isMoving)
            BuffPlayerUnit();

        if (IsBuffReady() && _isMoving)
            StopMoving();

        if (!IsBuffReady() && !_isMoving)
            _isMoving = true;
    }

    private void StopMoving()
    {
        if (_isMoving)
        {
            _waitingTime = Time.time;
            _isMoving = false;
        }
    }

    private bool IsBuffReady()
    {
        return (Time.time - _lastCastTime >= (_buffCooldown + _afterCastDelay)) || _lastCastTime == 0f;
    }

    private bool IsReadyToCast()
    {
        return (Time.time - _waitingTime >= _castTime) && _waitingTime != 0f;
    }

    private void BuffPlayerUnit()
    {
        var playerUnitsInRadius = GetUnitsInRadius(unit.Config.AttackRange, true);
        var playerUnitsWithoutBuffs = new List<IReadOnlyUnit>();

        if (playerUnitsInRadius.Any())
        {
            foreach (var unit in playerUnitsInRadius)
            {
                if (!_effectManager.HasActiveBuffs(unit))
                    playerUnitsWithoutBuffs.Add(unit);
            }
        }

        if (playerUnitsWithoutBuffs.Any())
        {
            var unit = playerUnitsWithoutBuffs[0];
            _effectManager.AddEffect(unit, new IncAttSpdEffect(unit));
            ServiceLocator.Get<VFXView>().PlayVFX(unit.Pos, VFXView.VFXType.BuffApplied);
        }

        _lastCastTime = Time.time;
    }
}
