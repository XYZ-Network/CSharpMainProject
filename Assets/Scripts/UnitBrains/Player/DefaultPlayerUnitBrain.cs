using System.Collections.Generic;
using Model;
using Model.Runtime.Projectiles;
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
            {
                return unit.Pos;
            }

            IReadOnlyUnit recomUnit = TargetRecommendationManager.Instance.RecommendationTarget;
            Vector2Int recomPos = TargetRecommendationManager.Instance.RecommendationPosition;

            if (IsDoubleDistanceAttack(recomPos))
            {
                recomPos = TargetRecommendationManager.Instance.RecommendationPosition;
            }

            if (unit.Pos.Equals(recomPos))
            {
                return unit.Pos;
            }

            _activePath = new PatAStar(runtimeModel, unit.Pos, recomPos);
            return _activePath.GetNextStepFrom(unit.Pos);
        }

        public bool IsDoubleDistanceAttack(Vector2Int target)
        {
            float distance = unit.Config.AttackRange + unit.Config.AttackRange;
            float distanceToTarget = Vector2Int.Distance(target, unit.Pos);
            return distance > distanceToTarget;
        }
    }
}