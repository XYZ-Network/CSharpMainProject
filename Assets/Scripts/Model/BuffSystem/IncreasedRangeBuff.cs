using Model.Runtime.ReadOnly;

namespace Model.BuffSystem.Buffs
{
    public class IncreasedRangeBuff : IBuff
    {
        private readonly float _duration;
        private readonly float _rangeMultiplier;
        private float _remainingTime;

        public bool IsExpired => _remainingTime <= 0f;

        public IncreasedRangeBuff(float duration, float rangeMultiplier = 1.5f)
        {
            _duration = duration;
            _rangeMultiplier = rangeMultiplier;
            _remainingTime = duration;
        }

        public bool CanApply(IReadOnlyUnit unit)
        {
            return unit.Config.Name == "Ironclad Behemoth";
        }

        public void Apply(IReadOnlyUnit unit)
        {
            // Реализация через ThirdUnitBrain
        }

        public void Reset(IReadOnlyUnit unit)
        {
            // Реализация через ThirdUnitBrain
        }

        public void Update(float deltaTime)
        {
            _remainingTime -= deltaTime;
        }
    }
}