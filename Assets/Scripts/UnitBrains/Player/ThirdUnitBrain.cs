using System;
using System.Collections.Generic;
using UnitBrains.Buffs;
using UnityEngine;

namespace UnitBrains.Player
{
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Ironclad Behemoth";

        private static int _reloadingTimeSeconds = 1;
        private static DateTime _reloadingStartDateTime;

        private bool _isReloading;
        private bool _isBurstMode;
        private bool _hasIncreasedRange = false;
        private float _originalAttackRange;
        private bool _isBuffApplied = false;

        public override Vector2Int GetNextStep()
        {
            if (NeedToSwitchMode()) StartModeSwitching();
            return _isReloading || _isBurstMode ? unit.Pos : base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            if (NeedToSwitchMode()) StartModeSwitching();

            // Если есть бафф увеличения радиуса — используем увеличенный радиус
            if (_hasIncreasedRange && _isBurstMode)
            {
                return GetTargetsWithIncreasedRange();
            }

            return _isReloading || !_isBurstMode ? new List<Vector2Int>() : base.SelectTargets();
        }

        private List<Vector2Int> GetTargetsWithIncreasedRange()
        {
            var result = new List<Vector2Int>();
            var originalRange = unit.Config.AttackRange;

            // Временно увеличиваем радиус для поиска целей
            var rangeField = typeof(Model.Config.UnitConfig).GetField("_attackRange",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (rangeField != null)
            {
                rangeField.SetValue(unit.Config, originalRange * 1.5f);
                result = base.SelectTargets();
                rangeField.SetValue(unit.Config, originalRange);
            }

            return result;
        }

        public override void Update(float deltaTime, float time)
        {
            if (_isReloading)
            {
                int totalSwitchingTime = (DateTime.Now - _reloadingStartDateTime).Seconds;
                if (totalSwitchingTime >= _reloadingTimeSeconds) CompleteModeSwitching();
            }

            // Проверяем бафф увеличения радиуса
            if (unit != null && !_isBuffApplied)
            {
                _hasIncreasedRange = BuffSystem.HasBuff(unit, "Increased Range");
                if (_hasIncreasedRange)
                {
                    _isBuffApplied = true;
                    Debug.Log($"ThirdUnitBrain: Increased Range buff active");
                }
            }
        }

        private bool NeedToSwitchMode() => !_isReloading && (HasTargetsInRange() != _isBurstMode);

        private void StartModeSwitching()
        {
            _reloadingStartDateTime = DateTime.Now;
            _isReloading = true;
        }

        private void CompleteModeSwitching()
        {
            _isBurstMode = !_isBurstMode;
            _isReloading = false;
        }

        // Публичный метод для наложения баффа (вызывается извне)
        public void ApplyIncreasedRangeBuff(float duration = 5f, float multiplier = 1.5f)
        {
            if (unit != null)
            {
                BuffSystem.ApplyBuff(unit, new IncreasedRangeBuff(duration, multiplier));
            }
        }

        public bool HasIncreasedRange => _hasIncreasedRange;
    }
}