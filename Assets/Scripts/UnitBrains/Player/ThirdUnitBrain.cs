using System.Collections.Generic;
using UnitBrains.Pathfinding;
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
		private bool hasTargets = false;
        private ArmyBrain _armyBrain;

        public override void SetArmyBrain(ArmyBrain armyBrain) => _armyBrain = armyBrain;

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
            
            var desired = hasTargets ? BrainMode.Attack : BrainMode.Move;
            
            if (_mode != desired)
            {
                BeginSwitch(desired);
            }
        }
        
        protected override List<Vector2Int> SelectTargets()
        {
            var recommended = _armyBrain?.GetRecommendedTarget();

            if (recommended != null)
            {
                float twoRanges = unit.Config.AttackRange * 2f;
                float dist = Vector2Int.Distance(unit.Pos, recommended.Pos);
                if (dist <= twoRanges && IsTargetInRange(recommended.Pos))
                {
                    hasTargets = true;
                    return new List<Vector2Int> { recommended.Pos };
                }
            }

            var result = base.SelectTargets();
            hasTargets = result.Count > 0;
            if (_mode != BrainMode.Attack)
                result.Clear();

            return result;
        }

        public override Vector2Int GetNextStep()
        {
            if (_mode != BrainMode.Move)
                return unit.Pos;

            var target = _armyBrain?.GetRecommendedPoint() ?? unit.Pos;
            if (target == unit.Pos)
                return unit.Pos;

            var path = new DummyUnitPath(runtimeModel, unit.Pos, target);
            return path.GetNextStepFrom(unit.Pos);
        }
    }
}
