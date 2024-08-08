using Model.Runtime;

namespace UnitBrains.Buff
{
    public class AttackPowerBuff : BaseBuff
    {
        public AttackPowerBuff(Unit unit, float duration, float modifier) 
            : base(unit, duration, modifier) {}

        public override void Activate()
        {
            base.Activate();
            unit.AttackModifier = modifier;
        }

        public override void Dispose()
        {
            base.Dispose();
            unit.AttackModifier = 1.0f;
        }
    }
}
