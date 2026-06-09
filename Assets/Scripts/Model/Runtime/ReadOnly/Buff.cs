using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Runtime;

public abstract class BuffBase
{
    public abstract bool CanApply(Unit unit);
    public abstract void Apply(Unit unit);
    public abstract void Remove(Unit unit);
}

public abstract class Buff<T> : BuffBase where T : Unit
{
    public override bool CanApply(Unit unit) =>
        unit is T typed && CanApplyTyped(typed);

    public override void Apply(Unit unit)
    {
        if (unit is T typed) ApplyTyped(typed);
    }

    public override void Remove(Unit unit)
    {
        if (unit is T typed) RemoveTyped(typed);
    }
    public abstract bool CanApplyTyped(T unit);
    public abstract void ApplyTyped(T unit);
    public abstract void RemoveTyped(T unit);
}