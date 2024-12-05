using Model.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeBufAt<T> : BuDebu<Unit>
{
    public float dDuration;
    public float dValue;
    public DeBufAt(float duration, float value) : base(duration, value)
    {
        dDuration = duration;
        dValue = value;
    }

    public override string NameOfBuff => "AtcDeBuff";

    public override void AddEffect(Unit unit)
    {
       unit.AtcCoef -= dValue;
    }

    public override void RemoveEffect(Unit unit)
    {
        unit.AtcCoef = 0;
    }
}
