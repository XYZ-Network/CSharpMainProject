using System.Collections.Generic;
using Model;
using UnitBrains.Pathfinding;
using UnityEngine;

namespace UnitBrains.Player
{
    public class DefaultPlayerUnitBrain : BaseUnitBrain
    {
        protected float DistanceToOwnBase(Vector2Int fromPos) =>
            Vector2Int.Distance(fromPos, runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]);

        public override Vector2Int GetNextStep()
        {
            UnitRecommendation recomm = UnitCoordinator.GetInstance().GetRecommendation(unit);
            Vector2Int? moveTarget = null;

            // If there's recommended target, and it's closer than 2x attack ranges (euclid distance), use it
            if (recomm.Target != null)
            {
                float distance = Vector2Int.Distance(unit.Pos, recomm.Target.Pos);

                if (distance < unit.Config.AttackRange * 2)
                {
                    moveTarget = recomm.Target.Pos;
                }
            }

            // if we don't have attack target, then move to recommended zone
            if (moveTarget == null)
            {
                moveTarget = recomm.Zone;
            }

            if (IsTargetInRange(moveTarget.Value))
            {
                _activePath = null;
                return unit.Pos;
            }

            _activePath = new AStarUnitPath(runtimeModel, unit.Pos, moveTarget.Value, IsPlayerUnitBrain);
            return _activePath.GetNextStepFrom(unit.Pos);
        }

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