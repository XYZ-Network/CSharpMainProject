using System.Collections.Generic;
using Assets.Scripts.UnitBrains.Player;
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
            if (HasTargetsInRange())
            {
                return unit.Pos;
            }
            IReadOnlyUnit recomendedUnit = TargetAdviser.Instance.RecomendedTarget;
            Vector2Int recomendedPosition = recomendedUnit == null ? TargetAdviser.Instance.EnemyBase : recomendedUnit.Pos;
            if (!IsTargetInDoubleRange(recomendedPosition))
            {
                recomendedPosition = TargetAdviser.Instance.RecomendedPosition;
            }
            if(unit.Pos.Equals(recomendedPosition)) {
                return recomendedPosition;
            }
            _activePath = new AStarUnitPath(runtimeModel, unit.Pos, recomendedPosition);
            return _activePath.GetNextStepFrom(unit.Pos);
        }
        protected bool IsTargetInDoubleRange(Vector2Int possibleTarget)
        {
            var attackRangeSqr = unit.Config.AttackRange * unit.Config.AttackRange * 2;
            var diff = possibleTarget - unit.Pos;
            return diff.sqrMagnitude * 2 < attackRangeSqr;
        }
    }
}