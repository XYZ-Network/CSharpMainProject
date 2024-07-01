using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Model.Runtime;

namespace BuffDebuff
{
    public class DoubleShotBuff : IBuff<Unit>
    {
        public float MoveSpeedModifier => 1.0f;
        public float AttackSpeedModifier => 1.0f;
        public float Duration { get; set; }

        string IBuff<Unit>.Name => throw new System.NotImplementedException();

        float IBuff<Unit>.MoveSpeedModifier { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        float IBuff<Unit>.AttackSpeedModifier { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public DoubleShotBuff(float duration)
        {
            Duration = duration;
        }

        public void Apply(Unit target)
        {
            target.EnableDoubleShot();
        }

        public void ApplyBuff(Unit target)
        {
            throw new System.NotImplementedException();
        }
    }
}


