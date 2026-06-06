using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnitBrains;
using UnitBrains.Player;
using UnityEngine;

namespace Baffs
{
    public class BuffAttackRange : BaseStatusEffect
    {
        public int Order => 1;

        public override void Effect(BaseUnitBrain brain)
        {
            ((ThirdUnitBrain)brain).AttackRange *= 2f;
        }

        public override void Diseffect(BaseUnitBrain brain)
        {
            ((ThirdUnitBrain)brain).AttackRange *= 0.5f;
        }

        virtual public bool CanAddStatusToData(List<List<StatusWithDestatusWithOrder>> buffs)
        {
            if (buffs.SelectMany(u => u).Select(u => u.Status).Any(u => u == Effect))
            {
                return false;
            }
            return true;
        }
    }

}
