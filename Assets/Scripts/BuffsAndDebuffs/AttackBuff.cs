using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Runtime;
using Model.Runtime.ReadOnly;
using UnitBrains;
using UnitBrains.Player;

namespace Assets.Scripts.BuffsAndDebuffs
{
    public class AttackBuff<T> : Effect<T> where T : Unit
    {
        public AttackBuff(T _unit) : base(_unit, EffectType.Attack)
        {
            Modifier = 1.5f;
            EffectDuration = 30f;
        }

        public override void ApplyEffect(T _owner, float modifier, float time)
        {
            var attackModifier = time + _owner.Config.AttackDelay / modifier;
            _owner.SetNextAttackTime(attackModifier);
        }

        public override void ClearEffect(T _owner)
        {
            _owner.SetNextAttackTime(null);
        }
    }
}
