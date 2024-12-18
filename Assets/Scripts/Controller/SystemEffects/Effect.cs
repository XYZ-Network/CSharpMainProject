using Model.Runtime;
using System.Collections;
using View;

public abstract class Effect<T> where T : Unit
{
    public float Duration;
    public float Reload;
    public VFXView _vFXView;

    //public virtual IEnumerator StartEffectToUnits(Unit unit,VFXView vFXView )
    //{
    //    unit.SetBuff(true);
    //    vFXView.PlayVFX(unit.Pos, VFXView.VFXType.BuffApplied);
    //    unit.SetAttackDelay(unit.AttackDelay * ModifierSpeedAttack) ;
    //    unit.SetMoveDelay(unit.MoveDelay * ModifierSpeedMove);

    //    yield return new WaitForSeconds(Duration);

    //    unit.SetAttackDelay(unit.Config.AttackDelay);
    //    unit.SetMoveDelay(unit.Config.MoveDelay);
    //    unit.SetBuff(false);
    //}

    public virtual bool SetEffect(T unit)
    {
        if (!CanSetEffect(unit))
        {
            return false;
        }
        Coroutines.Instance.StartCoroutine(UpdatingDurationEffect(unit));
        return true;
    }

    protected abstract IEnumerator UpdatingDurationEffect(T unit);
    protected virtual bool CanSetEffect(T unit)
    {
        return !unit.IsBuff;
    }

}
