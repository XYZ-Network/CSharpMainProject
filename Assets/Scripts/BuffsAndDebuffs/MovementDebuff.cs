using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Runtime;
using Model.Runtime.ReadOnly;


namespace Assets.Scripts.BuffsAndDebuffs
{
    public class MovementDebuff : Effect
    {
        public MovementDebuff(IReadOnlyUnit _unit) : base (_unit, EffectType.DMove)
        {
            Modifier = 0.5f;
            EffectDuration = 10f;

        }
    }
}
