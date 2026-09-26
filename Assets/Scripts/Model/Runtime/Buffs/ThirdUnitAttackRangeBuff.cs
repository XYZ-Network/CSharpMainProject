using System;

namespace Model.Runtime.Buffs
{
    public sealed class ThirdUnitAttackRangeBuff : Buff<ThirdUnit>
    {
        private readonly float _range;

        public ThirdUnitAttackRangeBuff(float duration, float range)
            : base(duration, "AttackRange")
        {
            if (range <= 0f)
                throw new ArgumentOutOfRangeException(nameof(range));

            _range = range;
        }

        protected override void OnApply(ThirdUnit unit)
        {
            unit.ModifyAttackRange(_range);
        }

        protected override void OnRemove(ThirdUnit unit)
        {
            unit.ModifyAttackRange(-_range);
        }
    }
}