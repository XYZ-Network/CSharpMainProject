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
    public class IncreaseRange : Buff<BaseUnitBrain>
    {
        public override bool IsBuff => true;
        private readonly float attackRangeMultiplier = 1.2f;
        public IncreaseRange() : base(5f, 1f, 1f) { }
        public IncreaseRange(float multiplier) : base(5f, 1f, 1f)
        {
            attackRangeMultiplier = multiplier;
        }
        public override bool CanBeAppliedTo(Unit unit) => unit.Brain is ThirdUnitBrain;
        public override void ApplyEffect(Unit unit)
        {
            unit.IncreaseAttackRange(attackRangeMultiplier);
        }
        public override void RemoveEffect(Unit unit)
        {
            Debug.Log("Here");
            unit.ResetAttackRange();
        }
    }
}
