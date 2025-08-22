using Assets.Scripts.UnitBrains.Pathfinding;
using Model;
using System.Collections.Generic;
using UnityEngine;

namespace UnitBrains.Player
{
    public class DefaultPlayerUnitBrain : BaseUnitBrain
    {
        private PlayerUnitCoordinator _coordinator;
        private PlayerUnitCoordinator Coordinator => _coordinator ??= PlayerUnitCoordinator.Instance;

        public override Vector2Int GetNextStep()
        {
            var target = GetNextStepTarget();

            if (IsTargetInRange(target))
                return unit.Pos;

            var destination = Coordinator.Destination;
            var moveTo = runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];

            _activePath = new AStarPathfinder(runtimeModel, unit.Pos, moveTo);
            
            return _activePath.GetNextStepFrom(unit.Pos);
        }

        public Vector2Int GetNextStepTarget()
        {
            return Coordinator.Target;
        }

        protected override List<Vector2Int> SelectTargets()
        {
            var suggestedTarget = GetNextStepTarget();

            if (IsTargetInRange(suggestedTarget))
                return new List<Vector2Int> { suggestedTarget };

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