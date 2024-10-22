using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootFasterBuff : IBuff
{
    public CharacterStats ApplyBuff(CharacterStats stats)
    {
        return new CharacterStats(stats.moveSpeed, stats.shootSpeed * 20);
    }
}
