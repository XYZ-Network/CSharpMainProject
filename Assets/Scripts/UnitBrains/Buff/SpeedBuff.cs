using Model.Runtime;

namespace UnitBrains.Buff
{
    public class SpeedBuff : BaseBuff
    {
        public SpeedBuff(Unit unit, float duration, float modifier) 
            : base(unit, duration, modifier) {}

        public override void Activate()
        {
            base.Activate();
            unit.SpeedModifier = modifier;
        }

        public override void Dispose()
        {
            base.Dispose();
            unit.SpeedModifier = 1.0f;
        }
    }
}
