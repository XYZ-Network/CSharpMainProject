using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UnitBrains.Buffs
{
    public class SlowdownMove : Buff
    {
        public SlowdownMove() : base(5f, 0.7f, 1) { }
    }
}
