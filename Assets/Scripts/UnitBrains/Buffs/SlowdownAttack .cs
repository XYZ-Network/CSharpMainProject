using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UnitBrains.Buffs
{
    public class SlowdownAttack : Buff
    {
        public SlowdownAttack() : base(5f, 1, 0.7f){ }
    }
}
