using System;

namespace Model.Runtime.Buffs
{
    public sealed class SpeedBuff<TUnit> : Buff<TUnit> where TUnit : Unit
    {
        private readonly float _moveSpeed;
        private readonly float _attackSpeed;

        public SpeedBuff(
            float duration,
            float moveSpeed,
            float attackSpeed)
            : base(duration, "SpeedBuff")
        {
            if (moveSpeed <= 0f)
                throw new ArgumentOutOfRangeException(nameof(moveSpeed));

            if (attackSpeed <= 0f)
                throw new ArgumentOutOfRangeException(nameof(attackSpeed));

            _moveSpeed = moveSpeed;
            _attackSpeed = attackSpeed;
        }

        protected override void OnApply(TUnit unit)
        {
            unit.ModifyMoveSpeed(_moveSpeed);
            unit.ModifyAttackSpeed(_attackSpeed);
        }

        protected override void OnRemove(TUnit unit)
        {
            unit.ModifyMoveSpeed(1f / _moveSpeed);
            unit.ModifyAttackSpeed(1f / _attackSpeed);
        }
    }
}