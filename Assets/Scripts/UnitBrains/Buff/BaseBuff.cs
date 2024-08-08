using Model.Runtime;
using System;

namespace UnitBrains.Buff
{
    public interface IBuffable : IDisposable
    {
        public void Activate();

        public bool CanApplyToUnit();

        public void ReduceDurationByDelta(float delta);
        public bool IsActive();
    }

    public class BaseBuff : IBuffable
    {
        public float Duration { get; set; }
        protected float modifier;
        protected Unit unit;

        public BaseBuff(Unit unit, float duration, float modifier) {
            this.unit = unit;
            this.Duration = duration;
            this.modifier = modifier;
        }

        public virtual void Activate()
        {
        }

        public virtual bool CanApplyToUnit()
        {
            return true;
        }

        public void ReduceDurationByDelta(float delta)
        {
            Duration -= delta;
        }

        public bool IsActive() => Duration > 0;

        public virtual void Dispose()
        {
        }
    }
}
