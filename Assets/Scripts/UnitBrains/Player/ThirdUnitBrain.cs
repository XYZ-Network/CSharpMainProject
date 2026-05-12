using System.Collections.Generic;
using UnityEngine;

namespace UnitBrains.Player
{
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Ironclad Behemoth";
        protected override Vector2Int RecommendedPointOffset => Vector2Int.left;

        private const float ModeSwitchDuration = 1f;

        private UnitMode _currentMode = UnitMode.Moving;
        private UnitMode _targetMode = UnitMode.Moving;
        private float _modeSwitchEndTime;

        private bool IsSwitching => _currentMode != _targetMode;

        public override void Update(float deltaTime, float time)
        {
            base.Update(deltaTime, time);

            if (IsSwitching)
            {
                if (time >= _modeSwitchEndTime)
                    _currentMode = _targetMode;

                return;
            }

            var desiredMode = HasTargetsInRange() || TryGetRecommendedTarget(out _)
                ? UnitMode.Attacking
                : UnitMode.Moving;
            if (desiredMode == _currentMode)
                return;

            _targetMode = desiredMode;
            _modeSwitchEndTime = time + ModeSwitchDuration;
        }

        public override Vector2Int GetNextStep()
        {
            if (IsSwitching || _currentMode == UnitMode.Attacking)
                return unit.Pos;

            return base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            if (IsSwitching || _currentMode == UnitMode.Moving)
                return new List<Vector2Int>();

            return base.SelectTargets();
        }

        private enum UnitMode
        {
            Moving,
            Attacking,
        }
    }
}
