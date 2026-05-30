using Model.Runtime;
using UnityEngine;

namespace UnitBrains.Buffs
{
    public interface IBuff
    {
        string BuffName { get; }
        float Duration { get; }
        bool CanApplyTo(Unit unit);
        void Apply(Unit unit);
        void Remove(Unit unit);
    }
}
