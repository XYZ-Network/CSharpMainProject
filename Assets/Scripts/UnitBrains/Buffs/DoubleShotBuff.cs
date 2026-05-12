using Model.Runtime;
using UnitBrains.Player;

namespace UnitBrains.Buffs
{
    public class DoubleShotBuff : Buff<SecondUnitBrain>
    {
        private const int ExtraProjectilesPerShot = 1;

        protected override void ApplyEffect(Unit unit)
        {
            unit.AddExtraProjectiles(ExtraProjectilesPerShot);
        }
    }
}
