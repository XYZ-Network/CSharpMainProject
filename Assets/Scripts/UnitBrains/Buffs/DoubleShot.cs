using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitBrains;
using UnityEngine;
using Model.Runtime;
using UnitBrains.Player;

namespace Assets.Scripts.UnitBrains.Buffs
{
    public class DoubleShot : Buff<BaseUnitBrain>
    {
        public override bool IsBuff => true;
        public DoubleShot() : base(5f, 1f, 1f) { }
        public override bool CanBeAppliedTo(Unit unit) => unit.Brain is SecondUnitBrain;
        public override void ApplyEffect(Unit unit)
        {
            unit.DoubleShotOn();
        }
        public override void RemoveEffect(Unit unit)
        {
            unit.DoubleShotOff();
        }
    }
}
