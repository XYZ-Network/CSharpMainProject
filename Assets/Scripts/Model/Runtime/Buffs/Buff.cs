using Model.Runtime;
using UnitBrains;

namespace Assets.Scripts.Model.Runtime.Buffs
{
    public abstract class Buff<TBrain> : IUnitBuff where TBrain : BaseUnitBrain
    {
        public float Duration { get; private set; }
        public float Modifier { get; }

        protected Buff(float duration, float modifier)
        {
            Duration = duration;
            Modifier = modifier;
        }

        bool IUnitBuff.Tick(float dt) => Tick(dt);
        bool IUnitBuff.CanApply(Unit u) => u.Brain is TBrain && CanApplyToUnit(u);
        protected virtual bool CanApplyToUnit(Unit _) => true;
        void IUnitBuff.ApplyTo(Unit u) => ApplyTo((TBrain)u.Brain, u);
        void IUnitBuff.RemoveFrom(Unit u) => RemoveFrom((TBrain)u.Brain, u);

        protected virtual bool Tick(float dt)
        {
            Duration -= dt;
            return Duration <= 0f;
        }

        protected abstract void ApplyTo(TBrain brain, Unit u);
        protected abstract void RemoveFrom(TBrain brain, Unit u);
    }
}