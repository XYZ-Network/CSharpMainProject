using UnityEngine;
using System.Collections.Generic;
using View;
using Model.Runtime;

public class SystemEffectsController : MonoBehaviour
{
    private VFXView _vFXView;
    private readonly Dictionary<NameEffects, Effect<Unit>> _allEffectsToUnit = new Dictionary<NameEffects, Effect<Unit>>();

    public void Constructor(VFXView vFXView)
    {
        _vFXView = vFXView;
        InitDictionary();
    }

    private void InitDictionary()
    {
        _allEffectsToUnit.Add(NameEffects.BuffAttackSpeed, new BuffAttackSpeed<Unit>());
        _allEffectsToUnit.Add(NameEffects.BuffMoveSpeed, new BuffMoveSpeed<Unit>());
        _allEffectsToUnit.Add(NameEffects.BuffDoubleShoot, new DoubleShootingEffect<Unit>());
        _allEffectsToUnit.Add(NameEffects.BuffAttackRange, new BuffAttackRange<Unit>());

        _allEffectsToUnit.Add(NameEffects.DebuffAttackSpeed, new DebuffAttackSpeed<Unit>());
        _allEffectsToUnit.Add(NameEffects.DebuffMoveSpeed, new DebuffMoveSpeed<Unit>());
    }

    public void StartEffectToUnit(Unit unit, NameEffects nameEffects, out float reload)
    {
        reload = 0f;
        if (unit == null)
        {
            return;
        }

        switch (unit.Config.Name)
        {
            case "Cobra Commando":
                nameEffects = NameEffects.BuffDoubleShoot;
                break;
            case "Ironclad Behemoth":
                nameEffects = NameEffects.BuffAttackRange;
                break;
        }

        if (_allEffectsToUnit[nameEffects].SetEffect(unit))
        {
            _vFXView.PlayVFX(unit.Pos, VFXView.VFXType.BuffApplied);
            reload = _allEffectsToUnit[nameEffects].Reload;
        }
    }
}

public enum NameEffects
{
    BuffDoubleShoot,
    BuffAttackRange,
    BuffAttackSpeed,
    BuffMoveSpeed,
    DebuffAttackSpeed,
    DebuffMoveSpeed,
}

