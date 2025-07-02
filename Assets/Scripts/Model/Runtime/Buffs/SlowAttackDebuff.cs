using Model.Runtime;
using UnitBrains;

namespace Assets.Scripts.Model.Runtime.Buffs
{
    // Замедление атаки
    public sealed class SlowAttackDebuff : Buff<BaseUnitBrain>
    {
        public SlowAttackDebuff(float duration, float multiplier)
            : base(duration, multiplier) { }

        protected override void ApplyTo(BaseUnitBrain brain, Unit u) => u.AddAttackSpeedMultiplier(Modifier);
        protected override void RemoveFrom(BaseUnitBrain brain, Unit u) => u.RemoveAttackSpeedMultiplier(Modifier);
    }
}