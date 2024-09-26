using Model.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Utilities.Buffs
{
    public class SlowdownBuff : AbstractBuff
    {
        public SlowdownBuff(Unit _unit) : base(_unit)
        {
            AttackDelayMod = 5.0f;
            duration = 3f;
        }
    }
}