using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
    public abstract class Buff<T> : IBuff<T>
    {
        public abstract string Name { get; }
        public abstract float Duration { get; set; }
        public abstract float MoveSpeedModifier { get; set; }
        public abstract float AttackSpeedModifier { get; set; }

        public abstract void Apply(T target);
        public abstract void ApplyBuff(T target);
    }
}
