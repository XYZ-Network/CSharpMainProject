using Model;
using Model.Runtime.Projectiles;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Pathfinding;
using UnityEngine;

namespace UnitBrains.Player
{
    public class DefaultPlayerUnitBrain : BaseUnitBrain
    {
        private BaseUnitPath _activePath = null;

        protected float DistanceToOwnBase(Vector2Int fromPos) =>
            Vector2Int.Distance(fromPos, runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]);

        protected void SortByDistanceToOwnBase(List<Vector2Int> list)
        {
            list.Sort(CompareByDistanceToOwnBase);
        }

        public override Vector2Int GetNextStep()
        {
            if (HasTargetsInRange())
                return unit.Pos;

            var target = unitCoordinator.GetRecomendedPos(IsPlayerUnitBrain);

            _activePath = new NewUnitPath(runtimeModel, unit.Pos, target);
            return _activePath.GetNextStepFrom(unit.Pos);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            var result = new List<Vector2Int>();
            var attackRange = this.unit.AttackRange;
            var target = unitCoordinator.GetRecomendedTarget(IsPlayerUnitBrain);
            if (GetUnitsInRadius(attackRange, false).Contains(target))
            {
                result.Add(target.Pos);
                return result;
            }

            return base.SelectTargets();
        }

        private int CompareByDistanceToOwnBase(Vector2Int a, Vector2Int b)
        {
            var distanceA = DistanceToOwnBase(a);
            var distanceB = DistanceToOwnBase(b);
            return distanceA.CompareTo(distanceB);
        }
    }
}