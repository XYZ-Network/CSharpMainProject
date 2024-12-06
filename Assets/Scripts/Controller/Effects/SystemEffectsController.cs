using UnityEngine;
using Model.Runtime;
using System.Collections.Generic;
using Model.Config;
using System.Collections;

public class SystemEffectsController : MonoBehaviour
{
    Dictionary<Unit, Effect> units = new Dictionary<Unit, Effect>();
    public void AddUnits(Unit unit, Effect effect)
    {
        unit.RequestEffect += StartEffect;
        units.Add(unit, effect);
    }

    public void StartEffect(Unit unit)
    {
        StartCoroutine(units[unit].StartEffect());
    }
}

public class Effect
{
    public float Duration;
    public float ModifierSpeedAttack;
    public float ModifierSpeedMove;

    private UnitConfig _unitConfig;

    public Effect(UnitConfig config, float duration, float modifierSpeedAttack, float modifierSpeedMove)
    {
        Duration = duration;
        ModifierSpeedAttack = modifierSpeedAttack;
        ModifierSpeedMove = modifierSpeedMove;

        _unitConfig = config;

    }

    public IEnumerator StartEffect()
    {
        _unitConfig.currentMoveDelay *= ModifierSpeedMove;
        _unitConfig.currentAttackDelay *= ModifierSpeedAttack;
        Debug.LogWarning(_unitConfig.currentAttackDelay + "  " + _unitConfig.currentMoveDelay);

        yield return new WaitForSeconds(Duration);

        _unitConfig.currentMoveDelay = _unitConfig.MoveDelay;
        _unitConfig.currentAttackDelay = _unitConfig.AttackDelay;
        Debug.LogError(_unitConfig.currentAttackDelay + "  " + _unitConfig.currentMoveDelay);
    }
}
