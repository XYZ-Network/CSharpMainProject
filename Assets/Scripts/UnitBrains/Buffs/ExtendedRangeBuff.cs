using Model.Runtime;
using UnitBrains.Player;

namespace UnitBrains.Buffs
{
    public class ExtendedRangeBuff : Buff<ThirdUnitBrain>
    {
        private const float AttackRangeBonus = 2f;

        protected override void ApplyEffect(Unit unit)
        {
            unit.AddAttackRangeBonus(AttackRangeBonus);
        }
    }
}
