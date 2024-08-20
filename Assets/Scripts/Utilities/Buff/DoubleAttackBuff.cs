using Model.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleAttackBuff : IBuff<Unit>
{
    public float Duration { get; set; } = 4f;
    public void Add(Unit unit)
    {
        unit.SetDoubleAttackActive(true);
    }
    public bool CanBeAdd(Unit unit)
    {
        return unit.Health > 0;
    }
    public void Remove(Unit unit)
    {
        unit.SetDoubleAttackActive(false);
    }
}
