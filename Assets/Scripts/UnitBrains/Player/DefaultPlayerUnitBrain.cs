using System.Collections.Generic;
using Model;
using Model.Runtime;
using UnitBrains.Pathfinding;
using UnityEngine;

namespace UnitBrains.Player
{
    public class DefaultPlayerUnitBrain : BaseUnitBrain
    {
        private BaseUnitPath _activePath;
        private int _attackRangeMultiplier = 2;

        public override Vector2Int GetNextStep()
        {
            var recommendedTarget = PlayerUnitCoordinator.GetInstance().RecommendedTarget;
            var recommendedPoint = PlayerUnitCoordinator.GetInstance().RecommendedPoint;

            if (RecommendedTargetNearby(recommendedTarget))
            {
                if (IsTargetInRange(recommendedTarget))
                    return unit.Pos;
                
                _activePath = new SmartUnitPath(runtimeModel, unit.Pos, recommendedTarget);
            }
            else
            {
                _activePath = new SmartUnitPath(runtimeModel, unit.Pos, recommendedPoint);
            }
            
            return _activePath.GetNextStepFrom(unit.Pos);
        }

        
        protected override List<Vector2Int> SelectTargets()
        {
            var result = GetReachableTargets();
            var recommendedTarget = PlayerUnitCoordinator.GetInstance().RecommendedTarget;
            
            if (result.Contains(recommendedTarget))
                return new List<Vector2Int>() { recommendedTarget };

            return new List<Vector2Int>();
        }
        
        
        private bool RecommendedTargetNearby(Vector2Int recomendedTargetPos)
        {
            var attackRangeSqr =  _attackRangeMultiplier * (unit.Config.AttackRange * unit.Config.AttackRange);
            var diff = recomendedTargetPos - unit.Pos;
            return diff.sqrMagnitude <= attackRangeSqr;
        }
    }
}