using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using UnitBrains.Pathfinding;
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
            Vector2Int recommendTarget = base.groupBrain.RecommendTarget;
            Vector2Int recommendPoint = base.groupBrain.RecommendPoint;

            float distanceToRecommendTarget = Vector2Int.Distance(recommendTarget, unit.Pos);

            Vector2Int activeTarget;

            if (distanceToRecommendTarget < 2 * unit.Config.AttackRange)
                activeTarget = recommendTarget;
            else
                activeTarget = recommendPoint;

            Vector2Int resultNextStep = unit.Pos;

            if (!base.IsTargetInRange(activeTarget))
            {
                AStarUnitPath astarPath = new AStarUnitPath(runtimeModel, unit.Pos, activeTarget, this);
                base.ActivePath = astarPath;
                resultNextStep = astarPath.GetNextStepFrom(unit.Pos);
            }

            return resultNextStep;
        }
    }
}