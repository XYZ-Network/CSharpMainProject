using UnityEngine;

namespace Systems.BuffSystem.Effects
{
    public class MoveSpeedBuff : IBuffEffect
    {
        public float Duration => 4f;
        private readonly float _multiplier;

        public MoveSpeedBuff(float multiplier)
        {
            _multiplier = multiplier;
        }

        public void Apply(ModifiableParams p)
        {
            Debug.Log($"MoveSpeedBuff {p.Current.MoveDelay}, {_multiplier}");
            p.Current.MoveDelay /= _multiplier;
        }
        
        public override string ToString()
        {
            return $"MoveSpeedBuff({_multiplier}, duration: {Duration})";
        }
    }
}