using UnityEngine;
using Model.Runtime;
using System.Collections.Generic;
using Model.Config;
using System.Collections;
using View;
using Model.Runtime.ReadOnly;

public class SystemEffectsController : MonoBehaviour
{
    Dictionary<NameEffects, Effect> allEffects = new Dictionary<NameEffects, Effect>();
    VFXView _vFXView;

    public void Constructor(VFXView vFXView)
    {
        _vFXView = vFXView;
    }

    public void AddEffect(NameEffects nameEffects, Effect effect)
    {
        if (!allEffects.ContainsKey(nameEffects))
        {
            allEffects.Add(nameEffects, effect);
        }
    }

    public void StartEffectToUnit(Unit unit, NameEffects nameEffects)
    {
        if (unit == null)
        {
            return;
        }

        StartCoroutine(allEffects[nameEffects].StartEffectToUnits(unit, _vFXView));
    }

    public Effect GetEffect(NameEffects name)
    {
        if (allEffects.ContainsKey(name))
            return allEffects[name];
        return null;
    }
}

public enum NameEffects
{
    BuffAttackSpeed,
    BuffMoveSpeed,
    DebuffAttackSpeed,
    DebuffMoveSpeed,
}

