using System.Collections;
using Model.Runtime;
using UnityEngine;

public class BuffAttackSpeed<T> : Effect<Unit>
{
    private readonly float _modifierSpeedAttack;
    public BuffAttackSpeed(float duration = 4f, float modifierSpeedAttack = 0.1f, float reload = 4f)
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
