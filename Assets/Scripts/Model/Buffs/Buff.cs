namespace Model.Buffs
{
    public class Buff
    {
        public BuffType Type;

        public float Duration;    // изначальная длительность (для удобства)
        public float TimeLeft;    // сколько осталось

        // Множители скорости (1 = без эффекта).
        // >1 — быстрее, <1 — медленнее.
        public float MoveSpeedMultiplier = 1f;
        public float AttackSpeedMultiplier = 1f;

        public bool IsExpired => TimeLeft <= 0f;
    }
}