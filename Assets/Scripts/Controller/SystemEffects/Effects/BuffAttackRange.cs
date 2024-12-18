using System.Collections;
using UnityEngine;
using Model.Runtime;

public class BuffAttackRange<T> : Effect<Unit>
{
    private readonly float _modifierAttackRange;
    public BuffAttackRange(float duration = 4f, float modifierAttackRange = 1.5f, float reload = 4f)
    {
        Duration = duration;
        _modifierAttackRange = modifierAttackRange;
        Reload = reload;
    }
    protected override IEnumerator UpdatingDurationEffect(Unit unit)
    {
        unit.SetBuff(true);
        unit.SetAttackRange(unit.AttackRange * _modifierAttackRange);
        yield return new WaitForSeconds(Duration);
        unit.SetBuff(false);
        unit.SetAttackRange(unit.Config.AttackRange);
    }

    protected override bool CanSetEffect(Unit unit)
    {
        return !unit.IsBuff;
    }
}
