using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UnitBrains;
using Assets.Scripts.UnitBrains.Pathfinding;
using Model;
using Model.Runtime.Projectiles;
using UnitBrains.Pathfinding;
using UnityEngine;

namespace UnitBrains.Player
{
    public class DefaultPlayerUnitBrain : BaseUnitBrain
    {
        public override BaseUnitPath ActivePath => _coordinatedPath;
        private BaseUnitPath _coordinatedPath;
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
                return unit.Pos;

            bool recomendationIsAvailable = false;
            if (_coordinator.RecomendedPosition != null)
                recomendationIsAvailable = CalculateDistance((Vector2Int)_coordinator.RecomendedPosition, unit.Pos) <= unit.Config.AttackRange + 2;

            var target = runtimeModel.RoMap.Bases[
                IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];

            if (_coordinator.RecomendedTarget != null)
            {
                if (recomendationIsAvailable)
                {
                    target = (Vector2Int)_coordinator.RecomendedPosition;
                    if (CalculateDistance(unit.Pos, target) <= 1)
                        return target;
                }
            }

            _coordinatedPath = new AStarUnitPath(runtimeModel, unit.Pos, target);
            return _coordinatedPath.GetNextStepFrom(unit.Pos);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            if (_coordinator.RecomendedTarget != null)
                if (IsTargetInRange((Vector2Int)_coordinator.RecomendedTarget))
                {
                    return new List<Vector2Int> { (Vector2Int)_coordinator.RecomendedTarget };
                }
            return base.SelectTargets();
        }

        private double CalculateDistance(Vector2Int vector1, Vector2Int vector2)
        {
            return Math.Max(Math.Abs(vector1.x - vector2.x), Math.Abs(vector1.y - vector2.y));
        }
    }
}