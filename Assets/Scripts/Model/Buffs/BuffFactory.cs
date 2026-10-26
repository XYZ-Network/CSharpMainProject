namespace Model.Buffs
{
    public static class BuffFactory
    {
        public static Buff MoveSpeedUp(float duration, float multiplier = 1.5f) => new()
        {
            Type = BuffType.MoveSpeedUp,
            Duration = duration,
            TimeLeft = duration,
            MoveSpeedMultiplier = multiplier,
        };

        public static Buff AttackSpeedUp(float duration, float multiplier = 1.5f) => new()
        {
            Type = BuffType.AttackSpeedUp,
            Duration = duration,
            TimeLeft = duration,
            AttackSpeedMultiplier = multiplier,
        };

        public static Buff MoveSpeedDown(float duration, float multiplier = 0.5f) => new()
        {
            Type = BuffType.MoveSpeedDown,
            Duration = duration,
            TimeLeft = duration,
            MoveSpeedMultiplier = multiplier,
        };

        public static Buff AttackSpeedDown(float duration, float multiplier = 0.5f) => new()
        {
            Type = BuffType.AttackSpeedDown,
            Duration = duration,
            TimeLeft = duration,
            AttackSpeedMultiplier = multiplier,
        };
    }
}