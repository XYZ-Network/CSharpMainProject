using Model.Runtime;
using Model.Runtime.ReadOnly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.BuffsDebuffsSystem
{
    public class IncMovSpdEffect : Effect
    {
        public IncMovSpdEffect(IReadOnlyUnit _unit) : base(_unit)
        {
            Modifier = 0.4f;
            Duration = 10;
        }
    }
}
