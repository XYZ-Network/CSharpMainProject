using System;

namespace Model.Runtime.Buffs
{
    public abstract class Buff<TUnit> : IBuff where TUnit : Unit
    {
        public string Id { get; }
        public bool IsExpired => _duration <= 0f;

        private float _duration;

        protected Buff(float duration, string id)
        {
            if (duration <= 0f)
                throw new ArgumentOutOfRangeException(nameof(duration));

            _duration = duration;
            Id = id;
        }

        public bool CanApply(Unit unit)
        {
            return unit is TUnit typedUnit && CanApply(typedUnit);
        }

        public void Apply(Unit unit)
        {
            if (unit is TUnit typedUnit)
                OnApply(typedUnit);
        }

        public void Remove(Unit unit)
        {
            if (unit is TUnit typedUnit)
                OnRemove(typedUnit);
        }

        public void Update(float deltaTime)
        {
            _duration -= deltaTime;
        }

        protected virtual bool CanApply(TUnit unit)
        {
            return unit != null && !unit.IsDead;
        }

        protected abstract void OnApply(TUnit unit);
        protected abstract void OnRemove(TUnit unit);
    }
}