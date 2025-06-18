namespace Model.Runtime.Buffs
{
    public class Buff
    {
        public enum BuffType
        {
            AttackSpeed,
            MoveSpeed,
            Invulnerability
        }
        
        public BuffType Type { get; }
        public float Duration { get; set; }
        public float Value { get; }

        // Value is relative
        // Positive buff value multiply base value (or devide delay)
        // Negative buff value devide base value (or multiply delay)
        public Buff(BuffType type, float duration, float value)
        {
            Type = type;
            Duration = duration;
            Value = value;
        }
    }
}