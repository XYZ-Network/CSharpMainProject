using Model.Runtime.ReadOnly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UnitBrains.Buffs.UnitBuffs
{
    internal class SecondUnitMultishotBuff : Buff<Third>
    {
        public SecondUnitMultishotBuff() : base("SecondMultishot", 1, 5)
        {
        }

        public override void Apply(IReadOnlyUnit unit)
        {
            unit.Modifiers.MaxTargets += unit.Config.MaxTargets * (int)Modifier;
        }

        public override void Remove(IReadOnlyUnit unit)
        {
            unit.Modifiers.MaxTargets -= unit.Config.MaxTargets * (int)Modifier;
        }
    }
}
