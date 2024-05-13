using System;
using AssemblyCSharp.Assets.Scripts.Controller;
using Model.Runtime;

namespace AssemblyCSharp.Assets.Scripts
{
    public class AttackSpeedBuff : IBuff<Unit>
    {
        private float _newAttackSpeed;

        public AttackSpeedBuff(float multiplier)
        {
            _newAttackSpeed = multiplier;
        }

        public bool CanApply(Unit unit)
        {

            return unit.Health > 0;
        }

        public void Apply(Unit unit)
        {

            unit.UpdateNextAttackTime(_newAttackSpeed);
        }
    }
}
