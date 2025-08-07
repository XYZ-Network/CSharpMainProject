using Model.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitBrains;
using UnitBrains.Player;
using UnityEngine;

namespace Assets.Scripts.UnitBrains.Buffs
{
    public class SlowdownAttack : Buff<BaseUnitBrain>
    {
        public override bool IsBuff =>  false;
        public SlowdownAttack() : base(5f, 1f, 0.8f) { }
    }
}
