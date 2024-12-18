using System.Collections;
using Model.Runtime;
using UnityEngine;

public class DebuffAttackSpeed<T> : Effect<Unit>
{
    private readonly float _modifierSpeedAttack;
    public DebuffAttackSpeed(float duration = 4f, float modifierSpeedAttack = 1.5f, float reload = 6f)
    {
        Duration = duration;
        _modifierSpeedAttack = modifierSpeedAttack;
        Reload = reload;
    }
    protected override IEnumerator UpdatingDurationEffect(Unit unit)
    {
        unit.SetBuff(true);
        unit.SetAttackDelay(unit.AttackDelay * _modifierSpeedAttack);
        yield return new WaitForSeconds(Duration);
        unit.SetBuff(false);
        unit.SetAttackDelay(unit.Config.AttackDelay);
    }
}
