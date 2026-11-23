using Model.Runtime.ReadOnly;

namespace Model.Runtime.Buffs
{
    public interface IBuff
    {
        public IReadOnlyUnit Unit { get; }
        
        public void ReduceRemainingTime(float time);
        public bool IsExpired();
        
        public void Apply();
        public void Remove();
    }
}