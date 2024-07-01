using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Model.Runtime;

namespace BuffDebuff
{
    public class IncreaseRangeBuff : IBuff<Unit>
    {
        public float MoveSpeedModifier => 1.0f;
        public float AttackSpeedModifier => 1.0f;
        public float Duration { get; set; }

        public string Name => throw new System.NotImplementedException();

        float IBuff<Unit>.MoveSpeedModifier { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        float IBuff<Unit>.AttackSpeedModifier { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        private readonly float _increaseAmount;
        private float _amount;

        public IncreaseRangeBuff(float increaseAmount, float duration)
        {
            _increaseAmount = increaseAmount;
            Duration = duration;
        }

        public void Apply(Unit target)
        {
            target.IncreaseAttackRange(_increaseAmount);
        }
        public IncreaseRangeBuff(float amount)
        {
            _amount = amount;
        }

        public void ApplyBuff(Unit target)
        {
            target.IncreaseAttackRange(_amount);
        }
    }
}


