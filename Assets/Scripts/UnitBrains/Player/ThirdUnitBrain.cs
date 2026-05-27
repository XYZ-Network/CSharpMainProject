using Model;
using System.Collections.Generic;
using UnitBrains.Pathfinding;
using UnityEngine;

namespace UnitBrains.Player
{
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        private const float SwitchDuration = 1f;

        private enum UnitMode
        {
            Moving,
            Attacking
        }

        private UnitMode _currentMode = UnitMode.Moving;
        private UnitMode _nextMode = UnitMode.Moving;
        private float _switchTimer = 0f;

        public override string TargetUnitName => "Ironclad Behemoth";

        private bool IsSwitching => _switchTimer > 0f;

        public override void Update(float deltaTime, float time)
        {
            base.Update(deltaTime, time);

            if (_switchTimer <= 0f)
                return;

            _switchTimer -= deltaTime;

            if (_switchTimer <= 0f)
            {
                _switchTimer = 0f;
                _currentMode = _nextMode;
            }
        }

        protected override List<Vector2Int> SelectTargets()
        {
            if (IsSwitching)
                return new List<Vector2Int>();

            if (_currentMode == UnitMode.Moving)
                return new List<Vector2Int>();

            List<Vector2Int> targets = base.SelectTargets();

            if (targets.Count == 0)
            {
                StartSwitch(UnitMode.Moving);
                return new List<Vector2Int>();
            }

            return targets;
        }

        public override Vector2Int GetNextStep()
        {
            if (IsSwitching)
                return unit.Pos;

            if (_currentMode == UnitMode.Attacking)
                return unit.Pos;

            List<Vector2Int> reachableTargets = GetReachableTargets();

            if (reachableTargets.Count > 0)
            {
                StartSwitch(UnitMode.Attacking);
                return unit.Pos;
            }

            Vector2Int fallbackTarget = runtimeModel.RoMap.Bases[
                IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];

            Vector2Int moveTarget = GetCoordinatorPointOrDefault(fallbackTarget);

            BaseUnitPath path = new AStarUnitPath(runtimeModel, unit.Pos, moveTarget);
            return path.GetNextStepFrom(unit.Pos);
        }

        private void StartSwitch(UnitMode nextMode)
        {
            _nextMode = nextMode;
            _switchTimer = SwitchDuration;
        }
    }
}