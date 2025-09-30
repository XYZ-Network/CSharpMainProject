using System.Collections.Generic;
using Model;
using Model.Runtime.Projectiles;
using UnitBrains.Pathfinding;
using UnityEngine;

namespace UnitBrains.Player
{
    public class DefaultPlayerUnitBrain : BaseUnitBrain
    {
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
            var attackDoubleRangeSqr = (unit.AttackRange * unit.AttackRange) * 2; //Вероятно и тут переписать
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
                target = unit.UnitCoordinator.GetTarget();
            else
                target = unit.UnitCoordinator.GetPoint();

            _activePath = new BrilliantUnitPath(runtimeModel, unit.Pos, target);
            return _activePath.GetNextStepFrom(unit.Pos);
        }
    }
}