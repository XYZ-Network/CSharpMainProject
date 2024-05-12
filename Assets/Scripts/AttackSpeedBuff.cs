using System;
using Model.Runtime;

namespace AssemblyCSharp.Assets.Scripts
{
    public class AttackSpeedBuff : AbsBuff<Unit> 
    {
        private float _newAttackSpeed;

        public AttackSpeedBuff(float multiplier)
        {
            _newAttackSpeed = multiplier;
        }

        public override bool CanApply(Unit unit)
        {

            return unit.Health > 0;
        }

        public override void Apply(Unit unit)
        {

            unit.UpdateNextAttackTime(_newAttackSpeed);
        }
    }
}
