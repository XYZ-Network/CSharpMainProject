using System.Collections.Generic;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;

namespace UnitBrains.Player
{
    public class DefaultPlayerUnitBrain : BaseUnitBrain
    {
        public override Vector2Int GetNextStep()
        {
            if (Coordinator == null || !Coordinator.IsInitialized)
                return base.GetNextStep();

            if (HasTargetsInRange())
                return unit.Pos;

            var recommendedPoint = Coordinator.RecomendedPoint;

            if (runtimeModel.IsTileWalkable(recommendedPoint))
            {
                return GetNextStepTowards(recommendedPoint);
            }

            var target = runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
            return GetNextStepTowards(target);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            if (Coordinator == null || !Coordinator.IsInitialized)
                return base.SelectTargets();

            var recommendedTarget = Coordinator.RecomendedTarget;

            if (Coordinator.IsTargetInRange(unit.Pos, recommendedTarget, unit.Config.AttackRange))
            {
                return new List<Vector2Int> { recommendedTarget };
            }

            return base.SelectTargets();
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
    }
}