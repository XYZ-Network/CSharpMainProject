using UnitBrains.Player;
using UnityEngine;

namespace Model.Runtime
{
    public class RangeBuff : Buff<Unit>
    {
        private ThirdUnitBrain _brain;
        private float _multiplier;
        private float _originalRange;

        public RangeBuff(float duration, float multiplier) : base(duration)
        {
            _multiplier = multiplier;
        }

        public override void Apply(Unit target)
        {
            _brain = target.GetBrain() as ThirdUnitBrain;
            if (_brain != null)
            {
                _originalRange = target.Config.AttackRange;
                _brain.ModifyAttackRange(_originalRange * _multiplier);
            }
  
        }

        public override void Remove(Unit target)
        {
            if (_brain != null)
            {
                _brain.ModifyAttackRange(_originalRange);
               
            }
        }
    }
}
