using System.Collections.Generic;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;
using UnitBrains.Coordinator;

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

        protected override List<Vector2Int> SelectTargets()
        {
            var coordinator = _coordinator;

            if (coordinator.RecommendedTarget.HasValue &&
                IsWithinDoubleAttackRange(coordinator.RecommendedTarget.Value))
            {
                return new List<Vector2Int> { coordinator.RecommendedTarget.Value };
            }
                        
            if (coordinator.RecommendedPoint.HasValue)
            {                
               _targetsToMove = new List<Vector2Int> { coordinator.RecommendedPoint.Value };
            }
            
            var result = GetReachableTargets();
            if (result.Count > 1)
                result.RemoveAt(result.Count - 1);

            return result;
        }


        public override Vector2Int GetNextStep()
        {
            var coordinator = _coordinator;
            if (coordinator.RecommendedPoint.HasValue)
                return coordinator.RecommendedPoint.Value;

            return base.GetNextStep();
        }

        private bool IsWithinDoubleAttackRange(Vector2Int pos)
        {
            float range = unit.Config.AttackRange;
            return (pos - unit.Pos).sqrMagnitude <= range * range * 4;
        }
    }
}