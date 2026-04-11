
using Assets.Scripts.Model.Runtime;
using UnitBrains;

namespace Model.Runtime.ReadOnly
{
    public interface IReadOnlyBuff
    {
        public int Duration { get; }
        public float Modifier { get; }
        public BuffType Type { get; }
        public bool IsBuffCanApply(System.Type unit);
        public void ApplyBuff(IWriteUnit unit);
        public void ClearBuff(IWriteUnit unit);
    }

    public enum BuffType
    {
        AttackSpeed,
        MoveSpeed,
        RangeIncrease,
        DoubleAttack
    }
}