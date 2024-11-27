using Model.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Effects
{
    public class DelayMoveSpeedEffect : BaseEffect<Unit>
    {
        public override string Name => "DelayMoveSpeedEffect";

        public override void AddEffect(Unit unit, float effectMultiplier)
        {
            unit._effectMoveTimeMultiplier = effectMultiplier;
        }

        public override void ClearEffect(Unit unit)
        {
            unit._effectMoveTimeMultiplier = 1;
        }
    }
}
