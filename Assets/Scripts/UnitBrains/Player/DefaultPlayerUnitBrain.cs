using Model;
using Model.Runtime.Projectiles;
using System.Collections.Generic;
using UnitBrains.Pathfinding;
using UnityEngine;

namespace UnitBrains.Player
{
    public class DefaultPlayerUnitBrain : BaseUnitBrain
    {
        private UnitCoordinator _coordinator;

        public void SetCoordinator(UnitCoordinator coordinator)
        {
            _coordinator = coordinator;
        }

        public override BaseUnitPath ActivePath => _activePath;
        private BaseUnitPath _activePath = null;
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

            bool HasTargetsInDoubleRange()
            {
                var attackDoubleRangeSqr = (unit.Config.AttackRange * unit.Config.AttackRange) * 2;
                foreach (var possibleTarget in GetAllTargets())
                {
                    var diff = possibleTarget - unit.Pos;
                    if (diff.sqrMagnitude < attackDoubleRangeSqr)
                        return true;
                }

                return false;
            }


            Vector2Int target = unit.Pos;

            if (HasTargetsInRange())
                return unit.Pos;

            if (HasTargetsInDoubleRange())
                target = _coordinator.GetTarget();
            else
                target = _coordinator.GetPoint();

            _activePath = new AStarUnitPath(runtimeModel, unit.Pos, target);
            return _activePath.GetNextStepFrom(unit.Pos);
        }
    }
}