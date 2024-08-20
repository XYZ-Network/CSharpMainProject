using Model.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackRangeBuff : IBuff<Unit>
{
    public float Duration { get; set; } = 2f;
    public float modifier = 10f;
    public void Add(Unit unit)
    {
        unit.ModifyAttackRange(modifier);
    }
    public bool CanBeAdd(Unit unit)
    {
        return unit.attackRangeModifier < modifier;
    }
    public void Remove(Unit unit)
    {
        unit.ModifyAttackRange(1f);
    }
}
