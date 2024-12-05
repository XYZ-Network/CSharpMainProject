using Model.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BufAt<T> : BuDebu<Unit>
{
    public float dDuration;
    public float dValue;
    public BufAt(float duration, float value) : base(duration, value)
    {
        Duration = duration;
        Value = value;
    }

    public override string NameOfBuff => "AtcBuff";

    public override void AddEffect(Unit unit)
    {
        unit.AtcCoef += Value;
    }

    public override void RemoveEffect(Unit unit)
    {
        unit.AtcCoef = 0;
    }
}
