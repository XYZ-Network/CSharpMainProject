using Model.Runtime;
using Model.Runtime.ReadOnly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.BuffsDebuffsSystem
{
    public class IncAttSpdEffect : Effect
    {
        public IncAttSpdEffect(IReadOnlyUnit _unit) : base(_unit)
        {
            Modifier = 0.01f;
            Duration = 5f;
        }
    }
}
