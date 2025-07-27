namespace Systems.BuffSystem.Effects
{
    public class AttackSpeedBuff : IBuffEffect
    {
        public float Duration => 3f;
        private readonly float _multiplier;

        public AttackSpeedBuff(float multiplier)
        {
            _multiplier = multiplier;
        }

        public void Apply(ModifiableParams p)
        {
            p.Current.AttackDelay /= _multiplier;
        }
        
        public override string ToString()
        {
            return $"AttackSpeedBuff({_multiplier}, duration: {Duration})";
        }
    }
}