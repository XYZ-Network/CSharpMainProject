
namespace Model.Runtime.ReadOnly
{
    public interface IReadOnlyBuff
    {
        public int Duration { get; }
        public float Modifier { get; }
        public BuffType Type { get; }

    }

    public enum BuffType
    {
        AttackSpeed,
        MoveSpeed
    }
}