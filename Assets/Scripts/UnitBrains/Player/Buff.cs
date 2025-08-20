using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UnitBrains.Player
{
    public class Buff
    {
        public float Duration;
        public float MoveSpeedModifier = 1f;
        public float AttackSpeedModifier = 1f;

        public Buff(float duration, float moveMod = 1f, float attackMod = 1f)
        {
            Duration = duration;
            MoveSpeedModifier = moveMod;
            AttackSpeedModifier = attackMod;
        }
    }
}
