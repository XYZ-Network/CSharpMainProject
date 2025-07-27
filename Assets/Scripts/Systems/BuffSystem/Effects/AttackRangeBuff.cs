using UnitBrains.Player;

namespace Systems.BuffSystem.Effects
{
    public class AttackRangeBuff : IBuffEffect, IConditionalBuff
    {
        public float Duration => 4f;
        private readonly float _multiplier;

        public AttackRangeBuff(float multiplier)
        {
            _multiplier = multiplier;
        }

        public void Apply(ModifiableParams p)
        {
            p.Current.AttackRange *= _multiplier;
        }

        public bool CanApplyTo(ModifiableParams p)
        {
            return p.Owner.Brain is ThirdUnitBrain;
        }

        public override string ToString()
        {
            return $"AttackRangeBuff({_multiplier}, duration: {Duration})";
        }
    }
}