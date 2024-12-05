using Model.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleAttack<T> : BuDebu<Unit>
{
    public float dDuration;
    public float dValue;
    public DoubleAttack(float duration, float value) : base(duration, value)
    {
        Duration = duration;
        Value = value;
    }

    public override string NameOfBuff => "DoubleBuff";

    public override void AddEffect(Unit unit)
    {
        unit.sing_or_duble_bullet = 2;
    }

    public override void RemoveEffect(Unit unit)
    {
        unit.sing_or_duble_bullet = 1;
    }
    public override bool CanUseEffect(Unit unit)
    {
        if (unit.Config.Name == "Cobra Commando")
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
