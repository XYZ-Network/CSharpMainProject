using Model.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Runtime;
using Utilities;

public class DoubleShootBuff : Buff<Unit>
{
    public override bool CanApplyTyped(Unit unit) =>
        !ServiceLocator.Get<BuffService>().HasBuff(unit, typeof(DoubleShootBuff));

    public override void ApplyTyped(Unit unit)
    {
        unit.SetBonusProjectiles(1);
    }

    public override void RemoveTyped(Unit unit)
    {
        unit.ResetBonusProjectiles();
    }
}