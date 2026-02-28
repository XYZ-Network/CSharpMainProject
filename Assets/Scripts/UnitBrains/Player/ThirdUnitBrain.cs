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
            Normal,    // Режим обычного поведения
            Switching, // Для перехода между режимами
            Siege      // Режим атаки цели
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

        // Для проверки на необходимость смены режима
        private void TryStartModeSwitchingIfNeeded()
        {
            if (ShouldSwitchMode())
                BeginModeSwitching();
        }

        // Логика для определения надо ли менять
        private bool ShouldSwitchMode()
        {
            bool hasTargets = HasTargetsInRange();
            bool inSiege = _mode == Mode.Siege;
            return _mode != Mode.Switching && (hasTargets != inSiege);
        }

        // Начало смены режима
        private void BeginModeSwitching()
        {
            _switchingStartTime = Time.time;
            _mode = Mode.Switching;
        }

        // Переход между режимами
        private void UpdateModeSwitching(float time)
        {
            if (_mode == Mode.Switching && (time - _switchingStartTime) >= SwitchingTimeSeconds)
                FinishModeSwitching();
        }

        // Конец смены режима
        private void FinishModeSwitching()
        {
            _mode = HasTargetsInRange() ? Mode.Siege : Mode.Normal;
        }
    }
}
