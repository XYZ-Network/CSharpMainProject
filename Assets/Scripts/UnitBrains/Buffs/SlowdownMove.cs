using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitBrains;
using UnityEngine;

namespace Assets.Scripts.UnitBrains.Buffs
{
    public class SlowdownMove : Buff<BaseUnitBrain>
    {
        public override bool IsBuff => false;
        public SlowdownMove() : base(5f, 0.7f, 1f) { }
    }
}
