using Model.Runtime.Projectiles;
using System.Collections.Generic;
using UnityEngine;

namespace UnitBrains.Player
{
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Ironclad Behemoth";

        private enum UnitMode
        {
            Driving,
            Shooting,
            Switching
        }

        private UnitMode _currentMode = UnitMode.Driving;
        private float _switchTimer = 0f;
        private const float SWITCH_DURATION = 1f;
        private UnitMode _nextMode;

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            if (_currentMode == UnitMode.Shooting)
                base.GenerateProjectiles(forTarget, intoList);
        }

        public override Vector2Int GetNextStep()
        {
            if (_currentMode == UnitMode.Switching || _currentMode == UnitMode.Shooting)
            {
                return unit.Pos;
            }
            return base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            if (_currentMode == UnitMode.Driving || _currentMode == UnitMode.Switching)
            {
                return new List<Vector2Int>();
            }
            return base.SelectTargets();
        }

        public override void Update(float deltaTime, float time)
        {
            switch (_currentMode)
            {
                case UnitMode.Switching:
                    if (time >= _switchTimer + SWITCH_DURATION)
                    {
                        _currentMode = _nextMode;
                    }
                    break;

                case UnitMode.Driving:
                    if (HasTargetsInRange())
                        SwitchTo(UnitMode.Shooting, time);
                    break;

                case UnitMode.Shooting:
                    if (!HasTargetsInRange())
                        SwitchTo(UnitMode.Driving, time);
                    break;
            }
        }

        private void SwitchTo(UnitMode newMode, float currentTime)
        {
            _currentMode = UnitMode.Switching;
            _nextMode = newMode;
            _switchTimer = currentTime;
        }
    }
}