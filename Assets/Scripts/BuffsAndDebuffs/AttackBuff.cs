using Model.Runtime;
using Model.Runtime.ReadOnly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.BuffsAndDebuffs
{
    public class AttackBuff : Effect
    {
        public AttackBuff(IReadOnlyUnit _unit) : base(_unit, EffectType.Attack)
        {
            Modifier = 4.5f;
            EffectDuration = 60f;
        }
    }
}
