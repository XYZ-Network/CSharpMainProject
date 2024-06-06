using Model.Runtime;
using Model.Runtime.ReadOnly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.BuffsDebuffsSystem
{
    public class DecAttSpdEffect : Effect
    {
        public DecAttSpdEffect(IReadOnlyUnit _unit) : base(_unit)
        {
            Modifier = 2f;
            Duration = 25f;
        }
    }
}
