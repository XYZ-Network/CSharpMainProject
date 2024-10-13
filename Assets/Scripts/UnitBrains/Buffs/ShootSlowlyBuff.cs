using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootSlowlyBuff: IBuff
{
    public CharacterStats ApplyBuff(CharacterStats stats)
    {
        return new CharacterStats(stats.moveSpeed, stats.shootSpeed / 2);
    }    
}
