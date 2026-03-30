
using Model.Runtime.ReadOnly;

namespace Model.Runtime
{
    
    public abstract class AbstractBuff : IReadOnlyBuff
    {
        private int _duration { get; }
        private float _modifier { get; }
        private BuffType _type { get; }

        int IReadOnlyBuff.Duration => _duration;

        float IReadOnlyBuff.Modifier => _modifier;

        BuffType IReadOnlyBuff.Type => _type;

        public AbstractBuff(int duration, float modifier, BuffType type)
        {
            _duration = duration;
            _modifier = modifier;
            _type = type;
        }

    }
}