namespace Buffs
{
    public sealed class UnitBuff
    {
        private const float DefaultDuration = 5f;

        public float Duration { get; private set; }
        public float MoveSpeedModifier { get; }
        public float AttackSpeedModifier { get; }

        public bool IsFinished => Duration <= 0f;

        public static UnitBuff MoveSpeedUp => new UnitBuff(DefaultDuration, 2f, 1f);
        public static UnitBuff AttackSpeedUp => new UnitBuff(DefaultDuration, 1f, 2f);
        public static UnitBuff MoveSpeedDown => new UnitBuff(DefaultDuration, 0.5f, 1f);
        public static UnitBuff AttackSpeedDown => new UnitBuff(DefaultDuration, 1f, 0.5f);

        public UnitBuff(float duration, float moveSpeedModifier, float attackSpeedModifier)
        {
            Duration = duration;
            MoveSpeedModifier = moveSpeedModifier;
            AttackSpeedModifier = attackSpeedModifier;
        }

        public void Tick(float deltaTime)
        {
            Duration -= deltaTime;
        }
    }
}