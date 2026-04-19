  using UnitBrains.Player;
using UnityEngine;

namespace Model.Runtime
{
    public class DoubleShotBuff : Buff<Unit>
    {
        private SecondUnitBrain _brain;

        public DoubleShotBuff(float duration) : base(duration)
        {
        }

        public override void Apply(Unit target)
        {
            _brain = target.GetBrain() as SecondUnitBrain;
            if (_brain != null)
            {
                _brain.EnableDoubleShot();

            }
        }

        public override void Remove(Unit target)
        {
            if (_brain != null)
            {
                _brain.DisableDoubleShot();

            }
        }
    }
}
