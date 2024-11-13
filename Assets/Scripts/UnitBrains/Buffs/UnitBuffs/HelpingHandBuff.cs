using Model.Runtime.ReadOnly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitBrains;

namespace Assets.Scripts.UnitBrains.Buffs
{
    internal class HelpingHandBuff : Buff<FourthUnitBrain>
    {
        public HelpingHandBuff() : base("HelpingHand", 0.3f, 5f)
        {
        }

        public override void Apply(IReadOnlyUnit unit)
        {
            unit.Modifiers.AttackDelay -= Modifier;
        }

        public override void Remove(IReadOnlyUnit unit)
        {
            throw new NotImplementedException();
        }
    }
}
