using Model.Runtime;
using Model.Runtime.ReadOnly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.BuffsDebuffsSystem
{
    public abstract class Effect
    {
        protected IReadOnlyUnit _unit;
        public float Modifier {  get; set; }
        public float Duration { get; set; }

        public Effect(IReadOnlyUnit unit)
        {
            _unit = unit;
        }
    }
}
