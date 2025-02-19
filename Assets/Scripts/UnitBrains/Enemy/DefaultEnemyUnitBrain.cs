using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using UnitBrains.Pathfinding;
using UnityEngine;

namespace UnitBrains.Enemy
{
    public class DefaultEnemyUnitBrain : BaseUnitBrain
    {
        public override bool IsPlayerUnitBrain => false;

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