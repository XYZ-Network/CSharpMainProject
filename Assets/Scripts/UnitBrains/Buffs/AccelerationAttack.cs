using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitBrains;
using UnityEngine;

namespace Assets.Scripts.UnitBrains.Buffs
{
    public class AccelerationAttack : Buff<BaseUnitBrain>
    {
        public AccelerationAttack() : base(5f, 1f, 1.5f) { }
    }
}
