namespace Model.Buffs
{
    public static class BuffFactory
    {
        public static MoveSpeedUpBuff MoveSpeedUp(float duration, float mult = 1.5f) => new(duration, mult);
        public static AttackSpeedUpBuff AttackSpeedUp(float duration, float mult = 1.5f) => new(duration, mult);
        public static MoveSpeedDownBuff MoveSpeedDown(float duration, float mult = 0.5f) => new(duration, mult);
        public static AttackSpeedDownBuff AttackSpeedDown(float duration, float mult = 0.5f) => new(duration, mult);

        public static DoubleShotBuff DoubleShot(float duration, float mult = 2f) => new(duration, mult);
        public static IncreasedRangeBuff IncreasedRange(float duration, float bonus = 2f) => new(duration, bonus);
    }
}