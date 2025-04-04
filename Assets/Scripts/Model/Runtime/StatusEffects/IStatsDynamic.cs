using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStatsDynamic
{
    public string GetName();
    
    public void ChangeDelayAttack(float modifier);

    public void ChangeMultiplierShot(int modifier);

    public void ChangeAttackRange(float modifier);
}
