using Model.Runtime;
using Model.Runtime.ReadOnly;

namespace UnitBrains.Buffs
{
    public abstract class BaseBuff
    {
        public abstract bool CanApplyTo(IReadOnlyUnit unit);
        public abstract void ApplyTo(Unit unit);
    }
}
