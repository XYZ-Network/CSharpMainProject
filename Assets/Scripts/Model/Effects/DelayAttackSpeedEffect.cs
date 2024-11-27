using Model.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Effects
{
    public class DelayAttackSpeedEffect : BaseEffect<Unit>
    {
        public override string Name => "DelayAttackSpeedEffect";

        public override void AddEffect(Unit unit, float effectMultiplier)
        {
            unit._effectAttackTimeMultiplier = effectMultiplier;
        }

        public override void ClearEffect(Unit unit)
        {
            unit._effectAttackTimeMultiplier = 1;
        }
    }
}
