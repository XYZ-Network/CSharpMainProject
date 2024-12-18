using System.Collections;
using Model.Runtime;
using UnityEngine;

public class BuffMoveSpeed<T> : Effect<Unit>
{
    private readonly float _modifierSpeedMove;
    public BuffMoveSpeed(float duration = 4f, float modifierSpeedMove = 0.1f, float reload = 4f)
    {
        Duration = duration;
        _modifierSpeedMove = modifierSpeedMove;
        Reload = reload;
    }
    protected override IEnumerator UpdatingDurationEffect(Unit unit)
    {
        unit.SetBuff(true);
        unit.SetMoveDelay(unit.AttackDelay * _modifierSpeedMove);
        yield return new WaitForSeconds(Duration);
        unit.SetBuff(false);
        unit.SetMoveDelay(unit.Config.AttackDelay);
    }

    protected override bool CanSetEffect(Unit unit)
    {
        return !unit.IsBuff;
    }
}
