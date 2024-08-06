using Model.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Utilities.BuffManager
{
    public class FireUpEffect : AbstractEffect
    {
        public FireUpEffect(Unit _unit) : base(_unit)
        {
            AttackDelayMod = 0.25f;
            duration = 5f;
        }
    }
}
