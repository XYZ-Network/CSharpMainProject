using Model.Runtime;
using UnityEngine;
using Utilities;

namespace UnitBrains.Buffs
{
    public abstract class BaseBuff<T> : IBuff where T : Unit
    {
        public string BuffName { get; protected set; }
        public float Duration { get; protected set; }
        protected TimeUtil TimeUtil => TimeUtil.Create();
        protected T TargetUnit { get; private set; }

        public virtual bool CanApplyTo(Unit unit)
        {
            return unit is T;
        }

        public virtual void Apply(Unit unit)
        {
            if (unit is T target)
            {
                TargetUnit = target;
                OnApply(target);

                if (Duration > 0)
                {
                    TimeUtil.RunDelayed(Duration, () => Remove(unit));
                }
            }
        }

        public virtual void Remove(Unit unit)
        {
            if (unit is T target)
            {
                OnRemove(target);
                TargetUnit = null;
            }
        }

        protected abstract void OnApply(T target);
        protected abstract void OnRemove(T target);
    }
}