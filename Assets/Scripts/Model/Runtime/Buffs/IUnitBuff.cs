using Model.Runtime;

namespace Assets.Scripts.Model.Runtime.Buffs
{
    public interface IUnitBuff
    {
        bool Tick(float deltaTime);
        bool CanApply(Unit unit);
        void ApplyTo(Unit unit);
        void RemoveFrom(Unit unit);
    }
}