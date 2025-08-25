using Model.Runtime.ReadOnly;

namespace Model.BuffSystem.Buffs
{
    public class DoubleShotBuff : IBuff
    {
        private readonly float _duration;
        private float _remainingTime;

        public bool IsExpired => _remainingTime <= 0f;

        public DoubleShotBuff(float duration)
        {
            _duration = duration;
            _remainingTime = duration;
        }

        public bool CanApply(IReadOnlyUnit unit)
        {
            return unit.Config.Name == "Cobra Commando";
        }

        public void Apply(IReadOnlyUnit unit)
        {
            // Реализация через SecondUnitBrain
        }

        public void Reset(IReadOnlyUnit unit)
        {
            // Реализация через SecondUnitBrain
        }

        public void Update(float deltaTime)
        {
            _remainingTime -= deltaTime;
        }
    }
}