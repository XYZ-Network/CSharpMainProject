using Assets.Scripts.Model.Runtime;
using Model;
using Model.Runtime.Projectiles;
using System.Collections.Generic;
using UnitBrains.Pathfinding;
using UnityEngine;
using static Codice.CM.Common.Merge.MergePathResolver;

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

            var recommendedPos = unitManager.RecommendedPosition;

            if (HasTargetsInRange())
                return unit.Pos;

            if (recommendedPos == Vector2Int.zero)
            {
                var fallback = runtimeModel.RoMap.Bases[
                    IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];

                _activePath = new SmartPath(runtimeModel, unit.Pos, fallback);
                return _activePath.GetNextStepFrom(unit.Pos);
            }

            _activePath = new SmartPath(runtimeModel, unit.Pos, recommendedPos);
            return _activePath.GetNextStepFrom(unit.Pos);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            var target = unitManager.RecommendedTarget;

            if (target == null)
                return new List<Vector2Int>();

            float dist = Vector2Int.Distance(unit.Pos, target.Pos);

            if (dist > unit.Config.AttackRange * 2)
                return new List<Vector2Int>();

            return new List<Vector2Int> { target.Pos };
        }
    }
}