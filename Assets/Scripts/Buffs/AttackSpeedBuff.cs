using Model.Runtime;

namespace Buffs
{
    public sealed class AttackSpeedBuff : TimedUnitBuff<Unit>
    {
        private readonly float _multiplier;

        public AttackSpeedBuff(float duration, float multiplier) : base(duration)
        {
            _multiplier = multiplier;
        }

        protected override void ApplyToTyped(Unit unit)
        {
            unit.MultiplyAttackSpeed(_multiplier);
        }
    }
}