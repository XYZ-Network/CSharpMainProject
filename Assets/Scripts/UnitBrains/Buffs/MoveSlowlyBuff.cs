using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSlowlyBuff : IBuff
{
    public CharacterStats ApplyBuff(CharacterStats stats)
    {
        return new CharacterStats(stats.moveSpeed / 2, stats.shootSpeed);
    }
}
