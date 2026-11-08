using Model.Runtime;
using UnitBrains;

namespace Model.Buffs
{
    /// <summary>
    /// Базовый не-обобщённый бафф: длительность, тик, применение/снятие.
    /// </summary>
    public abstract class Buff
    {
        public float Duration { get; }
        public float TimeLeft { get; private set; }
        public bool IsExpired => TimeLeft <= 0f;

        protected Buff(float duration)
        {
            Duration = duration;
            TimeLeft = duration;
        }

        public void Tick(float deltaTime) => TimeLeft -= deltaTime;

        /// <summary>
        /// Можно ли применить бафф к этому юниту.
        /// </summary>
        public abstract bool CanApplyTo(Unit unit);

        public abstract void Apply(Unit unit);
        public abstract void Remove(Unit unit);
    }

    /// <summary>
    /// Обобщённый бафф, знающий целевой тип юнита (по типу мозга).
    /// </summary>
    public abstract class Buff<TBrain> : Buff where TBrain : BaseUnitBrain
    {
        protected Buff(float duration) : base(duration) { }

        public override bool CanApplyTo(Unit unit) => unit.Brain is TBrain;

        public override void Apply(Unit unit)
        {
            if (unit.Brain is TBrain)
                ApplyTo(unit);
        }

        public override void Remove(Unit unit)
        {
            if (unit.Brain is TBrain)
                RemoveFrom(unit);
        }

        protected abstract void ApplyTo(Unit unit);
        protected abstract void RemoveFrom(Unit unit);
    }
}