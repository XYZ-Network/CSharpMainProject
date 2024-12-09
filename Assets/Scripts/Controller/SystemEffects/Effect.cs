
using Model.Config;
using Model.Runtime;
using System.Collections;
using UnityEngine;
using View;

public abstract class Effect
{
    public float Duration;
    public float Reload;
    public float ModifierSpeedAttack;
    public float ModifierSpeedMove;

    public Effect(float duration, float modifierSpeedAttack, float modifierSpeedMove, float reload)
    {
        Duration = duration;
        ModifierSpeedAttack = modifierSpeedAttack;
        ModifierSpeedMove = modifierSpeedMove;
        Reload = reload;
    }

    public virtual IEnumerator StartEffectToUnits(Unit unit,VFXView vFXView )
    {
        unit.isBuff = true;
        vFXView.PlayVFX(unit.Pos, VFXView.VFXType.BuffApplied);
        unit.Config.CurrentAttackDelay *= ModifierSpeedAttack;
        unit.Config.CurrentMoveDelay *= ModifierSpeedMove;

        yield return new WaitForSeconds(Duration);

        unit.Config.CurrentAttackDelay = unit.Config.AttackDelay;
        unit.Config.CurrentMoveDelay = unit.Config.MoveDelay;
        unit.isBuff = false;
    }

}
