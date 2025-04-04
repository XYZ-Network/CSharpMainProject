using Model.Runtime.StatusEffects;

namespace Model.Runtime.StatusEffects
{
    public abstract class TemplateStatusEffect<T> where T : IStatsDynamic
    {
        public StatusEffectType Type { get; protected set; }
        public float Modifier { get; protected set; }

        public float Duration { get; protected set; }

        public bool TimeIsOver => (Duration <= 0);

        public abstract void StartEffect(T unit);
        public abstract void EndEffect(T unit);

        public virtual bool CanApply(T unit)
        {
            return true;
        }
        public virtual void UpdateTimer(float deltaTime)
        {
            Duration -= deltaTime;
        }

        public override bool Equals(object obj)
        {
            if (obj is not TemplateStatusEffect<T> effect)
                return false;

            return Type == effect.Type;
        }

        public override int GetHashCode()
        {
            return (int)Type;
        }
    }
}