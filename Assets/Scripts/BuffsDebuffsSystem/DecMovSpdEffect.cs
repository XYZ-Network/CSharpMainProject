using Model.Runtime;
using Model.Runtime.ReadOnly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.BuffsDebuffsSystem
{
    public class DecMovSpdEffect : Effect
    {
        public DecMovSpdEffect(IReadOnlyUnit _unit) : base(_unit)
        {
            Modifier = 1.5f;
            Duration = 10;
        }
    }
}
