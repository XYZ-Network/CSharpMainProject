using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Runtime;
using Model.Runtime.ReadOnly;

namespace Assets.Scripts.BuffsAndDebuffs
{
    public class MovementBuff : Effect
    {
        public MovementBuff(IReadOnlyUnit _unit) : base (_unit, EffectType.Move)
        {
            Modifier = 10.5f;
            EffectDuration = 40f;
        }
    }
}
