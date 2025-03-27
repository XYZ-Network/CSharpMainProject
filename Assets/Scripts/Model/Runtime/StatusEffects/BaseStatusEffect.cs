using Model.Runtime.ReadOnly;

namespace Model.Runtime.StatusEffects
{
    public class BaseStatusEffect
    {
        public StatusEffectType Type { get; protected set; }
        public float Modifier;
        public float Duration;

        public BaseStatusEffect(StatusEffectType type, float duration, float modifier)
        {
            Type = type;
            Duration = duration;
            Modifier = modifier;
        }

        public BaseStatusEffect(StatusEffectType type)
        {
            Type = type;
            Duration = 0f;
            Modifier = 0f;
        }

        public override bool Equals(object obj)
        {
            if (obj is not BaseStatusEffect effect)
                return false;

            return Type == effect.Type;
        }

        public override int GetHashCode()
        {
            return (int)Type;
        }
    }
}
