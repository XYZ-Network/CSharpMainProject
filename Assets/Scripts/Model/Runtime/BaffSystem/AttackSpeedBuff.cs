using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffSystem
{
    public class AttackSpeedBuff : Buff<IModifiableUnit>
    {
        public AttackSpeedBuff(float duration, float modifier) : base(BuffType.AttackSpeed, duration, modifier) { }

        protected override void OnApply()
        {
            TargetUnit.ApplyBuffModifier(Type, Modifier);
        }

        protected override void OnUpdate(float deltaTime) { }

        protected override void OnRemove()
        {
            TargetUnit.RemoveBuffModifier(Type);
        }
    }
}