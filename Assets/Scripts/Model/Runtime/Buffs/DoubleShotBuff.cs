using Model.Runtime;
using UnitBrains.Player;

namespace Assets.Scripts.Model.Runtime.Buffs
{
    // Двойной залп
    public sealed class DoubleShotBuff : Buff<SecondUnitBrain>
    {
        private const int Multiplier = 2;

        public DoubleShotBuff(float duration)
            : base(duration, Multiplier) { }

        protected override void ApplyTo(SecondUnitBrain brain, Unit u) => u.AddProjectileMultiplier(Multiplier);
        protected override void RemoveFrom(SecondUnitBrain brain, Unit u) => u.RemoveProjectileMultiplier(Multiplier);
        public override string ToString() => $"DoubleShot(+{Multiplier})";
    }
}