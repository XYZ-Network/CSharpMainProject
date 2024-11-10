using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UnitBrains.Buffs
{
    internal class HelpingHandBuff : Buff
    {
        public HelpingHandBuff() : base("HelpingHand", BuffType.AttackSpeed, 0.3f, 5f)
        {
        }
    }
}
