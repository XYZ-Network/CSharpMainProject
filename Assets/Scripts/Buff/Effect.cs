using Model.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Buff
{
    public abstract class Effect
    {
        protected Unit _unit;
        public float Modifier { get; set; }
        public float Duration { get; set; }

        public Effect(Unit unit)
        {
            _unit = unit;
        }
    }
}