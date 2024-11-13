using Assets.Scripts.UnitBrains.Buffs;
using Model.Runtime;
using Model.Runtime.ReadOnly;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnitBrains;
using UnitBrains.Player;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{

}
public class FourthUnitBrain : DefaultPlayerUnitBrain
{
    public override string TargetUnitName => "War Cryer";

    private float _lastBuffTime;
    private bool IsCooldownPassed => Time.time - _lastBuffTime >= 0.5f;

    public override Vector2Int GetNextStep()
    {
        if (IsCooldownPassed)
        {
            return base.GetNextStep();
        }
        else
        {
            return unit.Pos;
        }
    }

    protected override List<Vector2Int> SelectTargets()
    {
        return new List<Vector2Int>();
    }

    public override void Update(float deltaTime, float time)
    {
        if (!IsCooldownPassed) { 
            return;
        }
        var buffTarget = GetBuffTarget();
        if (buffTarget != null)
        {
            BuffController.AddBuffToUnit(buffTarget, new HelpingHandBuff());
            _lastBuffTime = time;
        }
    }

    protected IReadOnlyUnit GetBuffTarget()
    {
        return runtimeModel.RoUnits
            .Where(u => u.Config.IsPlayerUnit)
            .Where(u => !TargetUnitName.Equals(u.Config.Name))
            .Where(u => (u.Pos - unit.Pos).sqrMagnitude < unit.Config.SquaredAttackRange)
            .Where(u => !BuffController.IsUnitHaveBuff<HelpingHandBuff>(u))
            .FirstOrDefault();
    }
}
