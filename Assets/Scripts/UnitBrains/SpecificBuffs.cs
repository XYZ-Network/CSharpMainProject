using Model.Runtime;
using System.Collections;
using UnityEngine;

namespace UnitBrains
{
    public class SpeedBuff : Buff<Unit>
    {
        private float _speedMultiplier;

        public SpeedBuff(float duration, float speedMultiplier) : base(duration)
        {
            _speedMultiplier = speedMultiplier;
        }
        
        public override void ApplyEffect(Unit unit)
        {
            unit.ModifyMoveDelay(_speedMultiplier);
        }

        public override bool CanApplyTo(Unit unit)
        {
            return unit.Config.Name.Contains("Sky Serpent"); ;
        }

        public override void RemoveEffect(Unit unit)
        {
            unit.ModifyMoveDelay(1f);
        }
    }

    public class DoubleAttackBuff : Buff<Unit>
    {
        public DoubleAttackBuff(float duration) : base(duration) { }

        public override void ApplyEffect(Unit unit)
        {
            unit.EnableDoubleAttack(true);
        }

        public override bool CanApplyTo(Unit unit)
        {
            return unit.Config.Name.Contains("Cobra Commando");
        }

        public override void RemoveEffect(Unit unit)
        {
            unit.EnableDoubleAttack(false);
        }
    }

    public class RangeBuff : Buff<Unit>
    {
        private float _rangeMultiplier;
        public RangeBuff(float duration, float rangeMultiplier) : base(duration)
        {
            _rangeMultiplier = rangeMultiplier;
        }

        public override void ApplyEffect(Unit unit)
        {
            unit.ModifyAttackRange(_rangeMultiplier);
        }

        public override bool CanApplyTo(Unit unit)
        {
            return unit.Config.Name.Contains("Ironclad Behemoth");
        }

        public override void RemoveEffect(Unit unit)
        {
            unit.ModifyAttackRange(1f);
        }
    }
}