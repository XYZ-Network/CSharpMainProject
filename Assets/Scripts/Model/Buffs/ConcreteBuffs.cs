using Model.Runtime;
using UnitBrains;
using UnitBrains.Player;

namespace Model.Buffs
{
    // ---------- Общие: работают на любом юните ----------

    public class MoveSpeedUpBuff : Buff<BaseUnitBrain>
    {
        private readonly float _multiplier;
        public MoveSpeedUpBuff(float duration, float multiplier = 1.5f) : base(duration)
            => _multiplier = multiplier;

        protected override void ApplyTo(Unit u) => u.AddMoveSpeedModifier(this, _multiplier);
        protected override void RemoveFrom(Unit u) => u.RemoveMoveSpeedModifier(this);
    }

    public class AttackSpeedUpBuff : Buff<BaseUnitBrain>
    {
        private readonly float _multiplier;
        public AttackSpeedUpBuff(float duration, float multiplier = 1.5f) : base(duration)
            => _multiplier = multiplier;

        protected override void ApplyTo(Unit u) => u.AddAttackSpeedModifier(this, _multiplier);
        protected override void RemoveFrom(Unit u) => u.RemoveAttackSpeedModifier(this);
    }

    public class MoveSpeedDownBuff : Buff<BaseUnitBrain>
    {
        private readonly float _multiplier;
        public MoveSpeedDownBuff(float duration, float multiplier = 0.5f) : base(duration)
            => _multiplier = multiplier;

        protected override void ApplyTo(Unit u) => u.AddMoveSpeedModifier(this, _multiplier);
        protected override void RemoveFrom(Unit u) => u.RemoveMoveSpeedModifier(this);
    }

    public class AttackSpeedDownBuff : Buff<BaseUnitBrain>
    {
        private readonly float _multiplier;
        public AttackSpeedDownBuff(float duration, float multiplier = 0.5f) : base(duration)
            => _multiplier = multiplier;

        protected override void ApplyTo(Unit u) => u.AddAttackSpeedModifier(this, _multiplier);
        protected override void RemoveFrom(Unit u) => u.RemoveAttackSpeedModifier(this);
    }

    // ---------- Новые: только для конкретных юнитов ----------

    /// <summary>Двойной выстрел — только для Cobra Commando (SecondUnitBrain).</summary>
    public class DoubleShotBuff : Buff<SecondUnitBrain>
    {
        private readonly float _multiplier;
        public DoubleShotBuff(float duration, float multiplier = 2f) : base(duration)
            => _multiplier = multiplier;

        protected override void ApplyTo(Unit u) => u.AddProjectileCountModifier(this, _multiplier);
        protected override void RemoveFrom(Unit u) => u.RemoveProjectileCountModifier(this);
    }

    /// <summary>Увеличение радиуса стрельбы — только для Ironclad Behemoth (ThirdUnitBrain).</summary>
    public class IncreasedRangeBuff : Buff<ThirdUnitBrain>
    {
        private readonly float _bonus;
        public IncreasedRangeBuff(float duration, float bonus = 2f) : base(duration)
            => _bonus = bonus;

        protected override void ApplyTo(Unit u) => u.AddAttackRangeModifier(this, _bonus);
        protected override void RemoveFrom(Unit u) => u.RemoveAttackRangeModifier(this);
    }
}