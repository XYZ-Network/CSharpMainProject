using Model.Runtime;
using Model.Runtime.ReadOnly;
using System.Collections;
using UnitBrains;
using UnityEngine;

namespace Assets.Scripts.Model.Runtime.Buff
{
    public class DoubleAttackBuff<T> : AbstractBuff where T : BaseUnitBrain
    {
        public DoubleAttackBuff() : base(10, 2, BuffType.DoubleAttack)
        {
        }

        public override void ApplyBuff(IWriteUnit unit)
        {
            ClearBuff(unit);
            unit.AttackCount = unit.AttackCount + (int)Modifier;
        }

        public override void ClearBuff(IWriteUnit unit)
        {
            unit.ResetAttackCountToDefault();
        }

        public override bool IsBuffCanApply(System.Type brainType)
        {
            return brainType == typeof(T);
        }
    }
}