using Model.Runtime;

namespace Assets.Scripts.Utilities.Buffs
{
    public class RapidFireBuff : AbstractBuff
    {
        public RapidFireBuff(Unit _unit) : base(_unit)
        {
            AttackDelayMod = 0.25f;
            duration = 1f;
        }
    }
}