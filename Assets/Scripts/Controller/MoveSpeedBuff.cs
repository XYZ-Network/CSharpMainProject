using System;
using Model.Runtime;

namespace AssemblyCSharp.Assets.Scripts.Controller
{
    public class MoveSpeedBuff: AbsBuff<Unit> 
    {
        private float _newMoveSpeed;

        public MoveSpeedBuff(float multiplier)
        {
            _newMoveSpeed = multiplier;
        }

        public override bool CanApply(Unit unit)
        {
            
            return unit.Health > 0; 
        }

        public override void Apply(Unit unit)
        {
           
            unit.UpdateNextMoveTime(_newMoveSpeed);
        }
    }
}
