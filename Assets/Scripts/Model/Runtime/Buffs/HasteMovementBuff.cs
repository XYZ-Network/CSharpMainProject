using Model.Runtime;
using UnitBrains;

namespace Assets.Scripts.Model.Runtime.Buffs
{
    // Ускорение передвижения
    public sealed class HasteMovementBuff : Buff<BaseUnitBrain>
    {
        public HasteMovementBuff(float duration, float multiplier)
            : base(duration, multiplier) { }

        protected override void ApplyTo(BaseUnitBrain brain, Unit u) => u.AddMoveSpeedMultiplier(Modifier);
        protected override void RemoveFrom(BaseUnitBrain brain, Unit u) => u.RemoveMoveSpeedMultiplier(Modifier);
    }
}