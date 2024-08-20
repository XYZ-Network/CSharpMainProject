using Model.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBuff : IBuff<Unit>
{
    public float Duration { get; set; } = 3f;
    private float _modifier = 0.5f;

    public void Add(Unit unit)
    {
        unit.SetMovementSpeedModifier(_modifier);
    }

    public void Remove(Unit unit)
    {
        unit.SetMovementSpeedModifier(1f);

    }
    public bool CanBeAdd(Unit unit)
    {
        return unit.movementSpeedModifier <= 1f;
    }
}
