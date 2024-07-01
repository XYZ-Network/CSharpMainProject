using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{


    public class BuffDebuff
    {
        public float Duration { get; set; }
        public float MoveSpeedModifier { get; set; }
        public float AttackSpeedModifier { get; set; }
        public string Name { get; set; }

        public BuffDebuff(float duration, float moveSpeedModifier, float attackSpeedModifier, string name)
        {
            Duration = duration;
            MoveSpeedModifier = moveSpeedModifier;
            AttackSpeedModifier = attackSpeedModifier;
            Name = name;
        }
    }
}

