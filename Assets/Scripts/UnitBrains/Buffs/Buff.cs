using System.Collections.Generic;
using Model.Runtime;
using Model.Runtime.ReadOnly;

namespace UnitBrains.Buffs
{
    public abstract class Buff<TBrain> : BaseBuff where TBrain : BaseUnitBrain, new()
    {
        private static readonly string _targetUnitName = new TBrain().TargetUnitName;

        private readonly HashSet<IReadOnlyUnit> _appliedUnits = new();

        public sealed override bool CanApplyTo(IReadOnlyUnit unit)
        {
            if (unit == null || unit.Health <= 0)
                return false;

            if (unit.Config.Name != _targetUnitName)
                return false;

            return !_appliedUnits.Contains(unit);
        }

        public sealed override void ApplyTo(Unit unit)
        {
            if (!CanApplyTo(unit))
                return;

            _appliedUnits.Add(unit);
            ApplyEffect(unit);
        }

        protected abstract void ApplyEffect(Unit unit);
    }
}
