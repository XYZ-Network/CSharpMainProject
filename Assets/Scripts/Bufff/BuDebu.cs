using Model.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BuDebu<T> where T: Unit
{
    public float Duration { get;  set; }
    public float Value { get; set; }
    public BuDebu(float duration, float value)
    {
        Duration = duration;
        Value = value;
    }

    public abstract string NameOfBuff { get; }
    public abstract void AddEffect(T unit);
    public abstract void RemoveEffect(T unit);
    public virtual bool CanUseEffect(T unit)
    {
        return true;
    }
    
}
