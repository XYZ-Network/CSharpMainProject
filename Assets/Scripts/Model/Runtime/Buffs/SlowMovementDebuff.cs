using Model.Runtime;
using UnitBrains;

namespace Assets.Scripts.Model.Runtime.Buffs
{
    // Замедление передвижения
    public sealed class SlowMovementDebuff : Buff<BaseUnitBrain>
    {
        public SlowMovementDebuff(float duration, float multiplier)
            : base(duration, multiplier) { }

        protected override void ApplyTo(BaseUnitBrain brain, Unit u)=> u.AddMoveSpeedMultiplier(Modifier);
        protected override void RemoveFrom(BaseUnitBrain brain, Unit u) => u.RemoveMoveSpeedMultiplier(Modifier);
    } 
}