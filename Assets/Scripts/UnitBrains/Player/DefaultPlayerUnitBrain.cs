using System.Collections.Generic;
using System.Linq;
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
        private Vector2Int _currentTarget;

        public override Vector2Int GetNextStep()
        {
            if (Time.time - _lastDecisionTime >= DecisionInterval)
            {
                _ignoreRecommendation = Random.value < IgnoreRecommendationChance;
                Vector2Int newTarget = _ignoreRecommendation
                    ? Coordinator.GetRecommendedPoint()
                    : Coordinator.GetRecommendedTarget();

                if (_path == null || newTarget != _currentTarget)
                {
                    _currentTarget = newTarget;
                    _path = new NewUnitPath(runtimeModel, unit.Pos, _currentTarget);
                }

                _lastDecisionTime = Time.time;
            }

            if (unit.Pos == _currentTarget)
                return unit.Pos;

            return _path?.GetNextStepFrom(unit.Pos) ?? unit.Pos;
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