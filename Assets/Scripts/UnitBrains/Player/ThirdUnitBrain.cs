using System.Collections.Generic;
using UnityEngine;

namespace UnitBrains.Player
{
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Ironclad Behemoth";

        private enum BrainMode { Move, Attack, Switching }

        private const float SwitchDuration = 1f;
        private BrainMode _mode = BrainMode.Move;
        private BrainMode _pendingMode = BrainMode.Move;
        private float _switchTimer = 0f;
        
        private void BeginSwitch(BrainMode to)
        {
            _pendingMode = to;
            _mode = BrainMode.Switching;
            _switchTimer = SwitchDuration;
        }

        public override void Update(float deltaTime, float time)
        {
            base.Update(deltaTime, time);
            
            if (_mode == BrainMode.Switching)
            {
                _switchTimer -= deltaTime;
                if (_switchTimer <= 0f)
                {
                    _mode = _pendingMode;
                    _switchTimer = 0f;
                }
                return;
            }
            
            bool hasTargetsInRange = HasTargetsInRange();
            var desired = hasTargetsInRange ? BrainMode.Attack : BrainMode.Move;
            
            if (_mode != desired)
            {
                BeginSwitch(desired);
            }
        }
        
        protected override List<Vector2Int> SelectTargets()
        {
            if (_mode != BrainMode.Attack)
                return new List<Vector2Int>();
            
            return base.SelectTargets();
        }
        
        public override Vector2Int GetNextStep()
        {
            if (_mode != BrainMode.Move)
                return unit.Pos;
            
            return base.GetNextStep();
        }
    }
}
