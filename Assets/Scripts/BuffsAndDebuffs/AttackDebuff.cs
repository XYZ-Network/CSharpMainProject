using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Runtime;
using Model.Runtime.ReadOnly;
using UnitBrains;

namespace Assets.Scripts.BuffsAndDebuffs
{
    public class AttackDebuff<T> : Effect<T> where T : Unit
    {
        public AttackDebuff(T _unit) : base(_unit, EffectType.Attack)
        {
            Modifier = 0.5f;
            EffectDuration = 30f;
        }

        public override void ApplyEffect(T owner, float modifier, float time)
        {
            var attackModifier = time + owner.Config.AttackDelay * modifier;
            owner.SetNextAttackTime(attackModifier);
        }

        public override void ClearEffect(T _owner)
        {
            _owner.SetNextAttackTime(null);
        }
    }
}
