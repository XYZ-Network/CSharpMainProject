using System.Collections.Generic;
using Model;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using UnityEngine;
using UnitBrains.Pathfinding;
using Assets.Scripts.UnitBrains.Player;

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
            IReadOnlyUnit recomendedUnit = recommendatedTarget.RecommendationTarget;
            Vector2Int recomendPosition = recommendatedTarget.RecommendationPosition;

            if (IsDoubleRangeAttack(recomendPosition))
            {
                recomendPosition = recommendatedTarget.RecommendationPosition;
            }

            if (unit.Pos.Equals(recomendPosition))
            {
                return unit.Pos;
            }

            _activePath = new NewPathFinding(runtimeModel, unit.Pos, recomendPosition);
            return _activePath.GetNextStepFrom(unit.Pos);
        }

        public bool IsDoubleRangeAttack(Vector2Int target)
        {
            float range = unit.Config.AttackRange + unit.Config.AttackRange;
            float distanceToTarget = Vector2Int.Distance(target, unit.Pos);
            return range > distanceToTarget;
        }
    }
}