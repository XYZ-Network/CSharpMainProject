using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace BuffDebuff
{
    public interface IBuff<T>
    {
        string Name { get; }
        float Duration { get; set; }
        float MoveSpeedModifier { get; set; }
        float AttackSpeedModifier { get; set; }

        void Apply(T target);
        void ApplyBuff(T target);
    }
}


