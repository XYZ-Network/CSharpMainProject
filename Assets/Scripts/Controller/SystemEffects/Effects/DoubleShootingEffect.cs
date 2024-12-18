using System.Collections;
using Model.Runtime;
using UnityEngine;


public class DoubleShootingEffect<T> : Effect<Unit>
{
    public DoubleShootingEffect(float duration = 6f, float reload = 4f)
    {
        Duration = duration;
        Reload = reload;
    }
    protected override IEnumerator UpdatingDurationEffect(Unit unit)
    {
        unit.SetBuff(true);
        unit.SetDoubleShoot(true);
        yield return new WaitForSeconds(Duration);
        unit.SetBuff(false);
        unit.SetDoubleShoot(false);
    }
}
