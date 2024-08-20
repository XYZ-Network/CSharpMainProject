using Model.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSpeedBuff : IBuff<Unit>
{
    public float Duration { get; set; } = 2f;
    private float _modifier = 0.1f;
    public void Add(Unit unit)
    {
        unit.SetAttackSpeedModifier(_modifier);
    }
    public bool CanBeAdd(Unit unit)
    {
        return unit.attackSpeedModifier <= 1f;
    }
    public void Remove(Unit unit)
    {
        unit.SetAttackSpeedModifier(1f);
    }
}
