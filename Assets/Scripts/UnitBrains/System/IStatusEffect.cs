using Assets.Scripts.UnitBrains.System;
using Model.Runtime;
using Model.Runtime.ReadOnly;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStatusEffect<T> : IStatusEffect where T : Enum
{
    new T Name { get; }
    new float Duration { get; }
    float Modificator {  get; }
    new public void BuffBreak(Unit unit);
}
public interface IStatusEffect
{
    Enum Name { get; }
    float Duration { get; }
    public void BuffBreak(Unit unit);

}
