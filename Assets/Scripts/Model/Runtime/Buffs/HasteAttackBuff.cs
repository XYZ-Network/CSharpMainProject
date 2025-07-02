using Model.Runtime;
using UnitBrains;
using UnitBrains.Player;

namespace Assets.Scripts.Model.Runtime.Buffs
{
    // Ускорение атаки
    public sealed class HasteAttackBuff : Buff<BaseUnitBrain>
    {
        public HasteAttackBuff(float duration, float multiplier)
            : base(duration, multiplier) { }
        protected override bool CanApplyToUnit(Unit u)
            => u.Brain.GetType() 
            == typeof(DefaultPlayerUnitBrain); // костыль
        protected override void ApplyTo(BaseUnitBrain brain, Unit u) => u.AddAttackSpeedMultiplier(Modifier);
        protected override void RemoveFrom(BaseUnitBrain brain, Unit u) => u.RemoveAttackSpeedMultiplier(Modifier);
        public override string ToString() => $"HasteAtk×{Modifier:g}";
    }
}