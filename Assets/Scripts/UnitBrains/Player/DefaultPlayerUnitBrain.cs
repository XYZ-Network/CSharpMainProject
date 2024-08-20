using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using UnitBrains.Pathfinding;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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
            if (GetAllTargets().Count() > 1)
            {
                if (HasRecommendTargetInRange())
                {
                    return unit.Pos;
                }

                _activePath = new AStarUnitPath(runtimeModel, unit.Pos, coordinator.getRecommendPos());
                return _activePath.GetNextStepFrom(unit.Pos);
            }

            var target = runtimeModel.RoMap.Bases[
                IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];

            _activePath = new AStarUnitPath(runtimeModel, unit.Pos, target);
            return _activePath.GetNextStepFrom(unit.Pos);

        }
        protected bool HasRecommendTargetInRange()
        {
            var attackRangeSqr = unit.Config.AttackRange * unit.Config.AttackRange;
            var diff = coordinator.getRecommendTarget() - unit.Pos;
            if (diff.sqrMagnitude < attackRangeSqr)
                return true;


            return false;
        }
        
    }
}