using System.Collections.Generic;
using Model;
using UnitBrains.Pathfinding;
using UnityEngine;

namespace UnitBrains.Player
{
    public class DefaultPlayerUnitBrain : BaseUnitBrain
    {
        
        public override Vector2Int GetNextStep()
        {
            var target = GetNextStepTarget();
            if (IsTargetInRange(target))
                return unit.Pos;

            var destination = coordinator.Destination;
            var moveTo = IsTargetInCoordinatorAcceptanceRange(target) ? target : destination;
            _activePath = new AStarUnitPath(runtimeModel, unit.Pos, moveTo);
            return _activePath.GetNextStepFrom(unit.Pos);
        }

        public override Vector2Int GetNextStepTarget() => coordinator.Target;

        protected override List<Vector2Int> SelectTargets()
        {
            var suggestedTarget = coordinator.Target;
            return IsTargetInRange(suggestedTarget)
                ? new List<Vector2Int> {suggestedTarget} : base.SelectTargets();
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