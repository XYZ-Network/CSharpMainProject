using System.Collections.Generic;
using System.Linq;
using Model;
using UnityEngine;

namespace UnitBrains.Player
{
    public class DefaultPlayerUnitBrain : BaseUnitBrain
    {
        protected virtual Vector2Int RecommendedPointOffset => Vector2Int.zero;

        public override Vector2Int GetNextStep()
        {
            if (TryGetRecommendedTarget(out _))
                return unit.Pos;

            var coordinator = PlayerUnitsCoordinator.Instance;
            if (coordinator.HasRecommendedPoint)
                return GetNextStepTo(FindClosestMovementPoint(coordinator.RecommendedPoint + RecommendedPointOffset));

            return base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            if (TryGetRecommendedTarget(out var recommendedTarget))
                return new List<Vector2Int> { recommendedTarget };

            return base.SelectTargets();
        }

        protected bool TryGetRecommendedTarget(out Vector2Int target)
        {
            target = default;
            var recommendedTarget = PlayerUnitsCoordinator.Instance.RecommendedTarget;
            if (recommendedTarget == null)
                return false;

            target = recommendedTarget.Pos;
            var attackWindow = unit.Config.AttackRange * 2f;
            var diff = target - unit.Pos;
            return diff.sqrMagnitude <= attackWindow * attackWindow;
        }

        protected float DistanceToOwnBase(Vector2Int fromPos) =>
            Vector2Int.Distance(fromPos, runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]);

        protected void SortByDistanceToOwnBase(List<Vector2Int> list)
        {
            list.Sort(CompareByDistanceToOwnBase);
        }

        private Vector2Int FindClosestMovementPoint(Vector2Int desiredPoint)
        {
            if (CanUseMovementPoint(desiredPoint))
                return desiredPoint;

            var maxDistance = Mathf.Max(runtimeModel.RoMap.Width, runtimeModel.RoMap.Height);
            for (int radius = 1; radius <= maxDistance; radius++)
            {
                foreach (var point in GetRing(desiredPoint, radius))
                {
                    if (CanUseMovementPoint(point))
                        return point;
                }
            }

            return desiredPoint;
        }

        private bool CanUseMovementPoint(Vector2Int point)
        {
            if (point == unit.Pos)
                return true;

            return !runtimeModel.RoMap[point] &&
                   runtimeModel.RoUnits.All(otherUnit => otherUnit == unit || otherUnit.Pos != point);
        }

        private static IEnumerable<Vector2Int> GetRing(Vector2Int center, int radius)
        {
            for (int shift = -radius; shift <= radius; shift++)
            {
                yield return center + new Vector2Int(shift, radius);
                yield return center + new Vector2Int(shift, -radius);
                yield return center + new Vector2Int(radius, shift);
                yield return center + new Vector2Int(-radius, shift);
            }
        }

        private int CompareByDistanceToOwnBase(Vector2Int a, Vector2Int b)
        {
            var distanceA = DistanceToOwnBase(a);
            var distanceB = DistanceToOwnBase(b);
            return distanceA.CompareTo(distanceB);
        }
    }
}
