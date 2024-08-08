using Model.Runtime;

namespace UnitBrains.Buff
{
    public class AttackRangeBuff : BaseBuff
    {
        public AttackRangeBuff(Unit unit, float duration, float modifier) 
            : base(unit, duration, modifier) {}

        public override void Activate()
        {
            base.Activate();
            unit.RangeModifier = modifier;
        }

        public override bool CanApplyToUnit()
        {
            return unit.Config.Name == "Ironclad Behemoth";
        }

        public override void Dispose()
        {
            base.Dispose();
            unit.RangeModifier = 1.0f;
        }
    }
}
