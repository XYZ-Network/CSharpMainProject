using Model.Runtime;
using Model.Runtime.ReadOnly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.BuffsAndDebuffs
{
    public class AttackDebuff : Effect
    {
        public AttackDebuff(IReadOnlyUnit _unit) : base(_unit, EffectType.DAttack)
        {
            Modifier = 0.5f;
            EffectDuration = 10f;
        }
    }
}
