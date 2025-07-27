using UnitBrains.Player;

namespace Systems.BuffSystem.Effects
{
    public class DoubleShotBuff : IBuffEffect, IConditionalBuff
    {
        public float Duration => 3f;
        
        public void Apply(ModifiableParams p)
        {
            p.Current.ProjectilesCount *= 2;
        }

        public bool CanApplyTo(ModifiableParams p)
        {
            return p.Owner.Brain is SecondUnitBrain;
        }
        
        public override string ToString()
        {
            return $"DoubleShotBuff(duration: {Duration})";
        }
    }
}