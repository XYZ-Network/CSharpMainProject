using Model.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Effects
{
    public class AttackExtraShootCountEffect : BaseEffect<Unit>
    {
        public override string Name => "AttackExtraShootCountEffect";
        public override void AddEffect(Unit unit, float ExtraShootCount)
        {
            if (!AbleAddEffect(unit)) return;
            unit._effectExtraShootCount = ExtraShootCount;
        }

        public override void ClearEffect(Unit unit)
        {
            unit._effectExtraShootCount = 0;
        }
        public override bool AbleAddEffect(Unit unit) 
        { 
            if (unit.Config.Name == "Cobra Commando") return true;
            return false;
        }
    }
}
