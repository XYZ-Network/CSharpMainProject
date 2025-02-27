using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
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
            int targetCount = runtimeModel.RoBotUnits.Count();
            if (targetCount > 1)
            {
                BaseUnitPath path;

                if(IsTargetInRange(unit._coordinator.RecTarget) || IsTargetInRange(unit._coordinator.RecPoint))
                {
                    return unit.Pos;
                }

                path = new MyUnitPath(runtimeModel, IsPlayerUnitBrain, unit.Pos, unit._coordinator.RecPoint);
                return path.GetNextStepFrom(unit.Pos);
            }
            else
            {
                return base.GetNextStep();
            }
        }
        protected override List<Vector2Int> SelectTargets()
        {
            int targetCount = runtimeModel.RoBotUnits.Count();
            int unitCount = runtimeModel.RoUnits.Count();
            Vector2Int recTarget= unit._coordinator.RecTarget;
            Vector2Int recPoint = unit._coordinator.RecPoint;

            if (IsTargetInRange(runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId]))
            {
                return base.SelectTargets();
            }
            else if (targetCount < 1)
            {
                return new List<Vector2Int>();
            }


            if (IsTargetInRange(recTarget))
            {
                unit._coordinator.Count++;
                if (unit._coordinator.Count > 1)
                {
                    return new List<Vector2Int> { recTarget };
                }
            }
            else if (IsTargetInRange(recPoint))
            {
                return base.SelectTargets();
            }
            else
            {
                return base.SelectTargets();
            }
            if(unitCount < 3)
            {
                return new List<Vector2Int> { recTarget };
            }
            return base.SelectTargets();
        }
            

    }
}