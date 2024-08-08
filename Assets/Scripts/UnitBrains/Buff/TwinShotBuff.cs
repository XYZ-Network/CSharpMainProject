using Model.Runtime;

namespace UnitBrains.Buff
{
    public class TwinShopBuff : BaseBuff
    {
        public TwinShopBuff(Unit unit, float duration, float modifier) 
            : base(unit, duration, modifier) {}

        public override void Activate()
        {
            base.Activate();
            unit.AttackTimeModifier = modifier;
        }

        public override bool CanApplyToUnit()
        {
            return unit.Config.Name == "Cobra Commando";
        }

        public override void Dispose()
        {
            base.Dispose();
            unit.AttackTimeModifier = 1.0f;
        }
    }
}
