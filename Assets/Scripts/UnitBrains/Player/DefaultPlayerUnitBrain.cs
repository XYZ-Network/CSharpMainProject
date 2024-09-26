using System.Collections.Generic;
using Assets.Scripts.UnitBrains.Pathfinding;
using Model;
using Model.Runtime.ReadOnly;
using UnityEngine;

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

        public override Vector2Int GetNextStep()
        {
            if (HasTargetsInRange())
                return unit.Pos;

            IReadOnlyUnit recommendTarget = _unitCoordinator.recommendTarget;
            if (recommendTarget != null)
            {
                SmartUnitPath path = new SmartUnitPath(runtimeModel, unit.Pos, recommendTarget.Pos);
                return path.GetNextStepFrom(unit.Pos);
            }

            return base.GetNextStep();
        }
    }
}