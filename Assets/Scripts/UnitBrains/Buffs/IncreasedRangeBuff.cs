using Model.Runtime;
using UnityEngine;

namespace UnitBrains.Buffs
{
    public class IncreasedRangeBuff : BaseBuff<Unit>
    {
        private bool _isActive = false;
        private float _originalAttackRange;
        private float _rangeMultiplier;

        public IncreasedRangeBuff(float duration = 5f, float multiplier = 1.5f)
        {
            BuffName = "Increased Range";
            Duration = duration;
            _rangeMultiplier = multiplier;
        }

        protected override void OnApply(Unit target)
        {
            _originalAttackRange = target.Config.AttackRange;

            // Увеличиваем радиус атаки через рефлексию
            var rangeField = typeof(Model.Config.UnitConfig).GetField("_attackRange",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (rangeField != null)
            {
                rangeField.SetValue(target.Config, _originalAttackRange * _rangeMultiplier);
                _isActive = true;
                Debug.Log($"IncreasedRangeBuff applied to {target.Config.Name}. New range: {target.Config.AttackRange}");
            }
        }

        protected override void OnRemove(Unit target)
        {
            if (!_isActive) return;

            var rangeField = typeof(Model.Config.UnitConfig).GetField("_attackRange",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (rangeField != null)
            {
                rangeField.SetValue(target.Config, _originalAttackRange);
                _isActive = false;
                Debug.Log($"IncreasedRangeBuff removed from {target.Config.Name}. Range restored to {target.Config.AttackRange}");
            }
        }
    }
}
