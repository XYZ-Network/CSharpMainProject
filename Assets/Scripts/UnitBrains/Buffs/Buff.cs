using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UnitBrains
{
    public class Buff
    {
        public float Duration { get; private set; }
        public float MoveModifier { get; private set; }
        public float AttackModifier { get; private set; }
        public Buff(float duration, float moveModifier, float attackModifier)
        {
            Duration = duration;
            MoveModifier = moveModifier;
            AttackModifier = attackModifier;
        }
        public void ReduceDuration()
        {
            Duration -= 0.1f;
        }
    }
}
