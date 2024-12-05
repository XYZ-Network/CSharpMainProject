using Model.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoreRange<T> : BuDebu<Unit>
{
    public float dDuration;
    public float dValue;
    public MoreRange(float duration, float value) : base(duration, value)
    {
        Duration = duration;
        Value = value;
    }

    public override string NameOfBuff => "RangeBuff";

    public override void AddEffect(Unit unit)
    {
        unit.Config.AttackRange =  Value;
     
    }

    public override void RemoveEffect(Unit unit)
    {
        unit.Config.AttackRange = 3.5f;
       
    }
    public override bool CanUseEffect(Unit unit)
    {
        if(unit.Config.Name == "Ironclad Behemoth")
        {
            return true;
        }
        else
        { 
            return false;
        }
    }
}
