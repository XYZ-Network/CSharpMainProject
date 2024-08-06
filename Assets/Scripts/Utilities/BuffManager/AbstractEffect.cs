using Model.Runtime;
using System.Collections;
using UnityEngine;
using Utilities;

public abstract class AbstractEffect 
{
    protected Unit _unit;
    public float duration { get; set; } = 3f;
    public float AttackDelayMod { get; set; } = 1f;


    public AbstractEffect(Unit unit) 
    {
        _unit = unit;
    }
}
