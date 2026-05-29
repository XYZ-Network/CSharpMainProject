using Model.Runtime;

namespace Buffs
{
    public sealed class DoubleShotBuff : TimedUnitBuff<Unit>
    {
        public DoubleShotBuff(float duration) : base(duration)
        {
        }

        protected override bool CanApplyToTyped(Unit unit)
        {
            return unit.Config.Name == "Cobra Commando";
        }

        protected override void ApplyToTyped(Unit unit)
        {
            unit.AddAdditionalShots(1);
        }
    }
}