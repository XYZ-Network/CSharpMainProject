using System.Collections.Generic;
using UnityEngine;

namespace UnitBrains.Player
{
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        enum UnitState
        {
            Moving,
            Attacking,
        }
        
        public override string TargetUnitName => "Ironclad Behemoth";
        private const float SwitchStateDurationSec = 1f;

        private UnitState _unitState = UnitState.Moving;
        private UnitState _targetUnitState = UnitState.Moving;

        private bool IsSwitchingState => _unitState != _targetUnitState;
        private float _switchStateStartTime = 0;

        public override void Update(float deltaTime, float time)
        {
            // Delta time is always 0.033 (30fps), so to use game time (not real time), we should use
            // `time` - seems it scales with `EnterPoint._timeScale`. Exactly what's needed.
            base.Update(deltaTime, time);

            if (IsSwitchingState)
            {
                if (time - _switchStateStartTime > SwitchStateDurationSec)
                {
                    _unitState = _targetUnitState;
                }

                return;
            }

            var hasTargets = HasTargetsInRange();
            
            if (_unitState == UnitState.Moving && hasTargets)
            {
                _targetUnitState = UnitState.Attacking;
                _switchStateStartTime = time;
            } 
            else if (_unitState == UnitState.Attacking && !hasTargets)
            {
                _targetUnitState = UnitState.Moving;
                _switchStateStartTime = time;
            }
        }

        public override Vector2Int GetNextStep()
        {
            return IsSwitchingState || _unitState == UnitState.Attacking
                ? unit.Pos
                : base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            return IsSwitchingState || _unitState == UnitState.Moving
                ? new List<Vector2Int>()
                : base.SelectTargets();
        }
    }
}