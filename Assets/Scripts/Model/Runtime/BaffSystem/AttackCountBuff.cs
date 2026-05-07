using BuffSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Codice.Client.BaseCommands.Import.Commit;

namespace BuffSystem
{
    public class AttackCountBuff : Buff<IModifiableUnit>
    {
        public AttackCountBuff(float duration, float modifier) : base(BuffType.AttackCount, duration, modifier) { }

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
