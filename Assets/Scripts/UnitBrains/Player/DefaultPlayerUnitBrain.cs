using System.Collections.Generic;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;
using Model.Runtime.ReadOnly;


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
            AStarPath path;
            if (HasTargetsInRange())
                return unit.Pos;
            var recTarg = UnitsCoordinator.Instance().ReccomedTarget();
            var recPoint = UnitsCoordinator.Instance().ReccomendPoint();
            if (recTarg != null && HasDoubleTargetsInRange(recTarg))
            {
                path = new AStarPath(runtimeModel, unit.Pos, recTarg.Pos);
                return path.GetNextStepFrom(unit.Pos);
            }

            var target = runtimeModel.RoMap.Bases[
               IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];

            path = new AStarPath(runtimeModel, unit.Pos, target);
            return path.GetNextStepFrom(unit.Pos);

        }
        protected bool HasDoubleTargetsInRange(IReadOnlyUnit targ)
        {
            var attackRangeSqr = 2 * unit.Config.AttackRange * unit.Config.AttackRange;
            var diff = targ.Pos - unit.Pos;
            if (diff.sqrMagnitude * 2 < attackRangeSqr)
                return true;
            return false;
        }
    }
}