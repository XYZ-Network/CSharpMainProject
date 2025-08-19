using System.Collections.Generic;
using Assets.Scripts.UnitBrains;
using Model;
using Model.Runtime.Projectiles;
using UnitBrains.Pathfinding;
using UnityEngine;

namespace UnitBrains.Player
{
    public class DefaultPlayerUnitBrain : BaseUnitBrain
    {
        private BaseUnitPath _path;
        private const float IgnoreRecommendationChance = 0.4f;
        private bool _ignoreRecommendation = false;
        private float _lastDecisionTime;
        private const float DecisionInterval = 2f;

        public override Vector2Int GetNextStep()
        {
            if (Time.time - _lastDecisionTime >= DecisionInterval)
            {
                _ignoreRecommendation = Random.value < IgnoreRecommendationChance;
                _lastDecisionTime = Time.time;
            }

            Vector2Int targetPoint;

            if (_ignoreRecommendation)
            {
                targetPoint = UnitCoordinator.Instance.GetRecommendedPoint();
            }
            else
            {
                targetPoint = UnitCoordinator.Instance.GetRecommendedTarget();
            }

            if (unit.Pos == targetPoint)
                return unit.Pos;

            _path = new NewUnitPath(runtimeModel, unit.Pos, targetPoint);
            return _path.GetNextStepFrom(unit.Pos);
        }

        public override BaseUnitPath ActivePath => _path;

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
    }
}