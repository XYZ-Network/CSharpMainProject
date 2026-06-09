using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Runtime;
using Utilities;

public class AttackRangeBuff : Buff<Unit>
{
    private const float RangeMultiplier = 2f; 

    public override bool CanApplyTyped(Unit unit) =>
        !ServiceLocator.Get<BuffService>().HasBuff(unit, typeof(AttackRangeBuff));

    public override void ApplyTyped(Unit unit)
    {
        unit.SetAttackRangeMultiplier(RangeMultiplier);
    }

    public override void RemoveTyped(Unit unit)
    {
        unit.ResetAttackRangeMultiplier();
    }
}