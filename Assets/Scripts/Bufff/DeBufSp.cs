using Model.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeBufSp<T> : BuDebu<Unit>
{
    public float dDuration;
    public float dValue;
    public DeBufSp(float duration, float value) : base(duration, value)
    {
        dDuration = duration;
        dValue = value;
    }

    public override string NameOfBuff => "SpdDeBuff";

    public override void AddEffect(Unit unit)
    {
        unit.MoveCoef -= dValue;
    }

    public override void RemoveEffect(Unit unit)
    {
        unit.MoveCoef = 0;
    }
}
