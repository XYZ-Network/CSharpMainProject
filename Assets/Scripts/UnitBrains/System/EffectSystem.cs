using Assets.Scripts.UnitBrains.System;
using Model.Runtime;
using Model.Runtime.ReadOnly;
using PlasticGui.WorkspaceWindow.PendingChanges;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;


public enum StatusType
{
    Buff,
    Debuff
}
public class EffectSystem : MonoBehaviour
{
    private Dictionary<IReadOnlyUnit, IStatusEffect> _effectsStatus = new();
    private Coroutine _coroutine;

    public Dictionary<IReadOnlyUnit, IStatusEffect> EffectStatus => _effectsStatus;
    public void UnitRegister(Unit unit)
    {

    }
    public void AddEffect(IReadOnlyUnit unit, StatusType type)
    {
        switch (type)
        {
            case StatusType.Buff:
                _effectsStatus[unit] = new BuffEffects((Unit)unit);
                Debug.LogWarning($"AddEffect{_effectsStatus[unit].Name}");
                break;
            case StatusType.Debuff:
                _effectsStatus[unit] = new DebuffEffects((Unit)unit);
                Debug.LogWarning($"AddEffect{_effectsStatus[unit].Name}");
                break;

        }
        StartCoroutine(EffectLifetime((Unit)unit));

    }
    private IEnumerator EffectLifetime(Unit unit)
    {
        
        yield return new WaitForSeconds(_effectsStatus[unit].Duration);
        Debug.LogWarning($"BreakEffect{_effectsStatus[unit].Name}");
        RemoveEffect(unit);
    }
    
    private void RemoveEffect(Unit unit)
    {
        _effectsStatus[unit].BuffBreak(unit);
        _effectsStatus.Remove(unit);

    }
}
