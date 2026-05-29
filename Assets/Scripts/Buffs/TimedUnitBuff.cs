using Model.Runtime.ReadOnly;

namespace Buffs
{
    public abstract class TimedUnitBuff<TUnit> : IUnitBuff
        where TUnit : class, IReadOnlyUnit
    {
        public float Duration { get; private set; }
        public bool IsFinished => Duration <= 0f;

        protected TimedUnitBuff(float duration)
        {
            Duration = duration;
        }

        public void Tick(float deltaTime)
        {
            Duration -= deltaTime;
        }

        public bool CanApplyTo(IReadOnlyUnit unit)
        {
            return unit is TUnit typedUnit && CanApplyToTyped(typedUnit);
        }

        public void ApplyTo(IReadOnlyUnit unit)
        {
            if (unit is TUnit typedUnit && CanApplyToTyped(typedUnit))
                ApplyToTyped(typedUnit);
        }

        protected virtual bool CanApplyToTyped(TUnit unit)
        {
            return true;
        }

        protected abstract void ApplyToTyped(TUnit unit);
    }
}