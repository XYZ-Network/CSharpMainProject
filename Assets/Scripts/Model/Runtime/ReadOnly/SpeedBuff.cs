using Model.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Runtime;
using Utilities;

public class SpeedBuff : Buff<Unit>
{
    private const float SpeedMultiplier = 0.5f;

    public override bool CanApplyTyped(Unit unit) =>
        !ServiceLocator.Get<BuffService>().HasBuff(unit, typeof(SpeedBuff));

    public override void ApplyTyped(Unit unit) =>
        unit.SetMoveDelayMultiplier(SpeedMultiplier);

    public override void RemoveTyped(Unit unit) =>
        unit.ResetMoveDelayMultiplier();
}