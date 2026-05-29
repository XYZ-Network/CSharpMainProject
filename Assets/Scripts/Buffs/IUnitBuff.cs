using Model.Runtime.ReadOnly;

namespace Buffs
{
    public interface IUnitBuff
    {
        bool IsFinished { get; }
        void Tick(float deltaTime);
        bool CanApplyTo(IReadOnlyUnit unit);
        void ApplyTo(IReadOnlyUnit unit);
    }
}