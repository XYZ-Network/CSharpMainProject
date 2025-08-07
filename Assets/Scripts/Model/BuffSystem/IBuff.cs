using Model.Runtime.ReadOnly;

namespace Model.BuffSystem
{
    public interface IBuff
    {
        bool CanApply(IReadOnlyUnit unit);
        void Apply(IReadOnlyUnit unit);
        void Reset(IReadOnlyUnit unit);
        void Update(float deltaTime);
        bool IsExpired { get; }
    }
}