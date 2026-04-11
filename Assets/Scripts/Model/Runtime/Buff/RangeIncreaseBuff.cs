using Model.Runtime;
using Model.Runtime.ReadOnly;
using UnitBrains;

namespace Assets.Scripts.Model.Runtime.Buff
{
    public class RangeIncreaseBuff<T> : AbstractBuff where T : BaseUnitBrain
    {
        public RangeIncreaseBuff() : base(10, 2, BuffType.RangeIncrease)
        {
        }

        public override void ApplyBuff(IWriteUnit unit)
        {
            ClearBuff(unit);
            unit.AttackRange = unit.AttackRange + Modifier;
        }

        public override void ClearBuff(IWriteUnit unit)
        {
            unit.ResetAttackRangeToDefault();
        }

        public override bool IsBuffCanApply(System.Type brainType)
        {
            return brainType == typeof(T);
        }

    }
}