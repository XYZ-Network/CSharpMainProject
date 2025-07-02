using Model.Runtime;
using UnitBrains.Player;
using UnityEngine;

namespace Assets.Scripts.Model.Runtime.Buffs
{
    // увеличение дальности
    public sealed class IncreaseRangeBuff : Buff<ThirdUnitBrain>
    {
        private const float DefaultMultiplier = 1.5f;

        public IncreaseRangeBuff(float duration, float multiplier = DefaultMultiplier)
            : base(duration, multiplier) { } 

        protected override void ApplyTo(ThirdUnitBrain brain, Unit u) => u.AddAttackRangeMultiplier(Modifier);
        protected override void RemoveFrom(ThirdUnitBrain brain, Unit u) => u.RemoveAttackRangeMultiplier(Modifier);
        public override string ToString() => $"IncreaseRange×{Modifier:g}";
    }
}