using Model.Runtime;
using UnityEngine;

namespace Buffs
{
    public sealed class AttackRangeBuff : TimedUnitBuff<Unit>
    {
        private readonly float _rangeBonus;

        public AttackRangeBuff(float duration, float rangeBonus) : base(duration)
        {
            _rangeBonus = rangeBonus;
        }

        protected override bool CanApplyToTyped(Unit unit)
        {
            return unit.Config.Name == "Ironclad Behemoth";
        }

        protected override void ApplyToTyped(Unit unit)
        {
            unit.AddAttackRange(_rangeBonus);
            Debug.Log($"{unit.Config.Name} current range = {unit.CurrentAttackRange}");
        }
    }
}