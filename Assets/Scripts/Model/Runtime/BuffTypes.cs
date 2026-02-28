using UnityEngine;

namespace Model.Runtime
{
    // Ускорение движения
    public class MovementSpeedBuff<T> : Buff<T> where T : Unit
    {
        public MovementSpeedBuff(float duration, float modifier) : base(duration, modifier) { }

        public override void Apply(T unit) => unit.SetMoveSpeedModifier(Modifier);
        public override void Remove(T unit) => unit.SetMoveSpeedModifier(1f);

        public override string GetDescription() =>
            $"Movement Speed Buff: x{Modifier} для {Duration:F1}s";
    }

    // Замедление движения
    public class MovementSlowDebuff<T> : Buff<T> where T : Unit
    {
        public MovementSlowDebuff(float duration, float modifier) : base(duration, modifier) { }

        public override void Apply(T unit) => unit.SetMoveSpeedModifier(Modifier);
        public override void Remove(T unit) => unit.SetMoveSpeedModifier(1f);

        public override string GetDescription() =>
            $"Movement Slow: x{Modifier} для {Duration:F1}s";
    }

    // Ускорение атаки
    public class AttackSpeedBuff<T> : Buff<T> where T : Unit
    {
        public AttackSpeedBuff(float duration, float modifier) : base(duration, modifier) { }

        public override void Apply(T unit) => unit.SetAttackSpeedModifier(Modifier);
        public override void Remove(T unit) => unit.SetAttackSpeedModifier(1f);

        public override string GetDescription() =>
            $"Attack Speed Buff: x{Modifier} для {Duration:F1}s";
    }

    // Замедление атаки
    public class AttackSlowDebuff<T> : Buff<T> where T : Unit
    {
        public AttackSlowDebuff(float duration, float modifier) : base(duration, modifier) { }

        public override void Apply(T unit) => unit.SetAttackSpeedModifier(Modifier);
        public override void Remove(T unit) => unit.SetAttackSpeedModifier(1f);

        public override string GetDescription() =>
            $"Attack Slow: x{Modifier} для {Duration:F1}s";
    }

    public class DoubleShotBuff<T> : Buff<T> where T : Unit
    {
        public DoubleShotBuff(float duration) : base(duration, 1f) { }

        public override void Apply(T unit) => unit.SetDoubleShot(true);
        public override void Remove(T unit) => unit.SetDoubleShot(false);

        public override string GetDescription() =>
            $"Double Shot для {Duration:F1}s";
    }

    // Увеличение радиуса — только для ThirdUnitBrain (Ironclad Behemoth)
    public class AttackRangeBuff<T> : Buff<T> where T : Unit
    {
        public AttackRangeBuff(float duration, float modifier) : base(duration, modifier) { }

        public override void Apply(T unit) => unit.SetAttackRangeModifier(Modifier);
        public override void Remove(T unit) => unit.SetAttackRangeModifier(1f);

        public override string GetDescription() =>
            $"Attack Range Buff: x{Modifier} для {Duration:F1}s";
    }
}