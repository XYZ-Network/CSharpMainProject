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

        public override void Update(float deltaTime, float time)
        {
            base.Update(deltaTime, time);
            UnitCoordinator.Instance.Update(deltaTime);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            var coordinator = UnitCoordinator.Instance;
            var recommendedTarget = coordinator.RecommendedTarget;

            if (recommendedTarget.HasValue && IsTargetInRange(recommendedTarget.Value))
            {
                return new List<Vector2Int> { recommendedTarget.Value };
            }

            if (coordinator.RecommendedPoint.HasValue)
            {
                _targetsToMove = new List<Vector2Int> { coordinator.RecommendedPoint.Value };
            }

            var result = GetReachableTargets();
            if (result.Count > 1)
                SortByDistanceToOwnBase(result);

            return result;
        }

        public override Vector2Int GetNextStep()
        {
            var coordinator = UnitCoordinator.Instance;
            if (coordinator.RecommendedPoint.HasValue)
            {
                var path = new AStarUnitPath(runtimeModel, unit.Pos, coordinator.RecommendedPoint.Value);
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