using System.Collections.Generic;
using Model.Runtime.Projectiles;
using UnityEngine;
using UnitBrains.Coordinator;
using Model;
using UnitBrains.Pathfinding;

namespace UnitBrains.Player
{
    public class DefaultPlayerUnitBrain : BaseUnitBrain
    {
        private UnitCoordinator _coordinator;

        public void SetCoordinator(UnitCoordinator coordinator)
        {
            _coordinator = coordinator;
        }

        protected float DistanceToOwnBase(Vector2Int fromPos) =>
            Vector2Int.Distance(fromPos, runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]);

        protected void SortByDistanceToOwnBase(List<Vector2Int> list)
        {
            list.Sort(CompareByDistanceToOwnBase);
        }

        private int CompareByDistanceToOwnBase(Vector2Int a, Vector2Int b)
        {
            var distanceA = DistanceToOwnBase(a);
            var distanceB = DistanceToOwnBase(b);
            return distanceA.CompareTo(distanceB);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            if (_coordinator == null)
                return base.SelectTargets();

            var recommendedTarget = _coordinator.RecommendedTarget;

            if (recommendedTarget.HasValue && IsTargetInRange(recommendedTarget.Value))
            {
                return new List<Vector2Int> { recommendedTarget.Value };
            }

            if (_coordinator.RecommendedPoint.HasValue)
            {
                _targetsToMove = new List<Vector2Int> { _coordinator.RecommendedPoint.Value };
            }

            var result = GetReachableTargets();
            if (result.Count > 1)
                SortByDistanceToOwnBase(result);

            return result;
        }

        public override Vector2Int GetNextStep()
        {
            if (_coordinator?.RecommendedPoint.HasValue ?? false)
            {
                var path = new AStarUnitPath(runtimeModel, unit.Pos, _coordinator.RecommendedPoint.Value);
                return path.GetNextStepFrom(unit.Pos);
            }

            var enemyBasePos = runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
            var pathToBase = new AStarUnitPath(runtimeModel, unit.Pos, enemyBasePos);
            return pathToBase.GetNextStepFrom(unit.Pos);
        }

        private bool IsWithinDoubleAttackRange(Vector2Int pos)
        {
            float range = unit.Config.AttackRange;
            return (pos - unit.Pos).sqrMagnitude <= range * range * 4;
        }
    }
}