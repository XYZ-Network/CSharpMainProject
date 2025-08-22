using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitBrains.Player
{
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Ironclad Behemoth";

        private const float SwitchingTimeSeconds = 1f;
        private float _switchingStartTime;
        
        private enum Mode
        {
            Normal,    // Обычное поведение
            Switching, // Переход между режимами
            Siege      // Атака цели
        }
        
        private Mode _mode = Mode.Normal;
        
        public override Vector2Int GetNextStep()
        {
            TryStartModeSwitchingIfNeeded();
            return _mode == Mode.Switching || _mode == Mode.Siege ? unit.Pos : base.GetNextStep();
        }
        
        protected override List<Vector2Int> SelectTargets()
        {
            TryStartModeSwitchingIfNeeded();
            return _mode == Mode.Siege ? base.SelectTargets() : new List<Vector2Int>();
        }

        public override void Update(float deltaTime, float time)
        {
            UpdateModeSwitching(time);
        }

        // Проверяет необходимость смены режима
        private void TryStartModeSwitchingIfNeeded()
        {
            if (ShouldSwitchMode())
                BeginModeSwitching();
        }

        // Логика определения необходимости смены режима
        private bool ShouldSwitchMode()
        {
            bool hasTargets = HasTargetsInRange();
            bool inSiege = _mode == Mode.Siege;
            return _mode != Mode.Switching && (hasTargets != inSiege);
        }
        
        // Инициирует процесс смены режима
        private void BeginModeSwitching()
        {
            _switchingStartTime = Time.time;
            _mode = Mode.Switching;
        }

        // Управляет переходом между режимами
        private void UpdateModeSwitching(float time)
        {
            if (_mode == Mode.Switching && (time - _switchingStartTime) >= SwitchingTimeSeconds)
                FinishModeSwitching();
        }

        // Завершает смену режима
        private void FinishModeSwitching()
        {
            _mode = HasTargetsInRange() ? Mode.Siege : Mode.Normal;
        }
    }
}