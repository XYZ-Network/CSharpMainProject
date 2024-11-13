using Model.Runtime.ReadOnly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UnitBrains.Buffs.UnitBuffs
{
    internal class ThirdUnitIcreaseRangeBuff : Buff<Third>
    {
        public ThirdUnitIcreaseRangeBuff() : base("ThirdRange", 1, 5)
        {
        }

        public override void Apply(IReadOnlyUnit unit)
        {
            unit.Modifiers.AttackRange += unit.Config.AttackRange * Modifier;
        }

        public override void Remove(IReadOnlyUnit unit)
        {
            unit.Modifiers.AttackRange -= unit.Config.AttackRange;
        }
    }
}
