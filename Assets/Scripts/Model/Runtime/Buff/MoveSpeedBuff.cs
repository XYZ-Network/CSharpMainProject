using Model.Runtime;
using Model.Runtime.ReadOnly;

namespace Assets.Scripts.Model.Runtime.Buff
{
    public class MoveSpeedBuff : AbstractBuff
    {
        public MoveSpeedBuff() : base(3, -1f, BuffType.MoveSpeed)
        {
        }
    }
}