using Model.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Effects
{
    public class RangeAttackEffect : BaseEffect<Unit>
    {
        public override string Name => "RangeAttackEffect";
        public override void AddEffect(Unit unit, float effectMultiplier)
        {
            if (!AbleAddEffect(unit)) return;
            unit._effectRangeMultiplier = effectMultiplier;
        }

        public override void ClearEffect(Unit unit)
        {
            unit._effectRangeMultiplier = 1;
        }
        public override bool AbleAddEffect(Unit unit)
        {
            if (unit.Config.Name == "Ironclad Behemoth") return true;
            return false;
        } 
    }
}
    

