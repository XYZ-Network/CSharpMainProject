using Model.Runtime;
using Model.Runtime.ReadOnly;

namespace Assets.Scripts.Model.Runtime.Buff
{
    public class MoveSpeedBuff : AbstractBuff
    {
        public MoveSpeedBuff() : base(3, -1f, BuffType.MoveSpeed)
        {
        }

        public override void ApplyBuff(IWriteUnit unit)
        {
            ClearBuff(unit);
            unit.MoveDelay = unit.MoveDelay + Modifier;
        }

        public override void ClearBuff(IWriteUnit unit)
        {
            unit.ResetMoveDelayToDefault();
        }
    }
}