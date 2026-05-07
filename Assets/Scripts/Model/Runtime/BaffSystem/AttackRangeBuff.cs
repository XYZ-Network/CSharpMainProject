using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffSystem
{
    public class AttackRangeBuff : Buff<IModifiableUnit>
    {
        public AttackRangeBuff(float duration, float modifier) : base(BuffType.AttackRange, duration, modifier) { }

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