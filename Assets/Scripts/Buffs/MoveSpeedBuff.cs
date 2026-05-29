using Model.Runtime;

namespace Buffs
{
    public sealed class MoveSpeedBuff : TimedUnitBuff<Unit>
    {
        private readonly float _multiplier;

        public MoveSpeedBuff(float duration, float multiplier) : base(duration)
        {
            _multiplier = multiplier;
        }

        protected override void ApplyToTyped(Unit unit)
        {
            unit.MultiplyMoveSpeed(_multiplier);
        }
    }
}