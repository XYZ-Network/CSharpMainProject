using Model.Config;
using Model.Runtime;
using UnityEngine;

namespace UnitBrains.Buffs
{
    public class DoubleShotBuff : BaseBuff<Unit>
    {
        private bool _isActive = false;
        private int _originalDamage;
        private float _originalAttackDelay;

        public DoubleShotBuff(float duration = 5f)
        {
            BuffName = "Double Shot";
            Duration = duration;
        }

        protected override void OnApply(Unit target)
        {
            _originalDamage = target.Config.Damage;
            _originalAttackDelay = target.Config.AttackDelay;

            // Увеличиваем урон в 2 раза
            var configField = typeof(UnitConfig).GetField("_damage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            configField?.SetValue(target.Config, _originalDamage * 2);

            // Уменьшаем задержку атаки (стреляет чаще)
            var delayField = typeof(UnitConfig).GetField("_attackDelay", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            delayField?.SetValue(target.Config, _originalAttackDelay * 0.5f);

            _isActive = true;
            Debug.Log($"DoubleShotBuff applied to {target.Config.Name}");
        }

        protected override void OnRemove(Unit target)
        {
            if (!_isActive) return;

            // Возвращаем исходные значения
            var configField = typeof(UnitConfig).GetField("_damage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            configField?.SetValue(target.Config, _originalDamage);

            var delayField = typeof(UnitConfig).GetField("_attackDelay", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            delayField?.SetValue(target.Config, _originalAttackDelay);

            _isActive = false;
            Debug.Log($"DoubleShotBuff removed from {target.Config.Name}");
        }
    }
}
