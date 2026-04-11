
using Assets.Scripts.Model.Runtime;
using Model.Runtime.ReadOnly;
using UnitBrains;

namespace Model.Runtime
{
    
    public abstract class AbstractBuff : IReadOnlyBuff
    {
        private int _duration { get; }
        private float _modifier { get; }
        private BuffType _type { get; }

        public float Modifier => _modifier;

        public BuffType Type => _type;

        public int Duration => _duration;

        public AbstractBuff(int duration, float modifier, BuffType type)
        {
            _duration = duration;
            _modifier = modifier;
            _type = type;
        }

        public abstract void ApplyBuff(IWriteUnit unit);
        public abstract void ClearBuff(IWriteUnit unit);
        public virtual bool IsBuffCanApply(System.Type brainType)
        { 
            return true; 
        }
    }
}