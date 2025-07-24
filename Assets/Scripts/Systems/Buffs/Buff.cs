namespace Utilities
{
    public class Buff
    {
        public float Duration { get; }
        public float SpeedMultiplier { get; }
        public float AttackSpeedMultiplier { get; }

        public Buff(float duration, float speedMod, float attackSpeedMod)
        {
            Duration = duration;
            SpeedMultiplier = speedMod;
            AttackSpeedMultiplier = attackSpeedMod;
        }

        public override string ToString()
        {
            return $"Buff(Dur: {Duration}, SpeedMult: {SpeedMultiplier}, AttackSpeedMult: {AttackSpeedMultiplier})";
        }

        public static Buff SpeedUp(float duration, float multiplier) => new(duration, multiplier, 1f);
        public static Buff SlowDown(float duration, float multiplier) => new(duration, multiplier, 1f);
        public static Buff AttackSpeedUp(float duration, float multiplier) => new(duration, 1f, multiplier);
        public static Buff AttackSlowDown(float duration, float multiplier) => new(duration, 1f, multiplier);
    }
}