using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnitBrains;
using UnitBrains.Player;
using UnityEngine;

namespace Baffs
{
    public class BuffSecondAttack : BaseStatusEffect
    {
        public int Order => 1;

        public override void Effect(BaseUnitBrain brain)
        {
            ((SecondUnitBrain)brain).SetSecondBullet(true);
        }

        public override void Diseffect(BaseUnitBrain brain)
        {
            ((SecondUnitBrain)brain).SetSecondBullet(false);
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
