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
    public class MovementDebuff<T> : Effect<T> where T : Unit
    {
        public MovementDebuff(T _unit) : base(_unit, EffectType.Move)
        {
            Modifier = 0.5f;
            EffectDuration = 30f;
        }

        public override void ApplyEffect(T _owner, float modifier, float time)
        {
            var moveModifier = time + _owner.Config.MoveDelay * modifier;
            _owner.SetNextMoveTime(moveModifier);
        }

        public override void ClearEffect(T _owner)
        {
            _owner.SetNextMoveTime(null);
        }
    }
}