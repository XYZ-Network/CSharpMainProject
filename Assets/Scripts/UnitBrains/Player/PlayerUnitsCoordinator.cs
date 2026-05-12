using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.ReadOnly;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    public sealed class PlayerUnitsCoordinator
    {
        private const float RecommendationRefreshDelay = 0.1f;
        private const float DefaultAttackRange = 3.5f;

        private static PlayerUnitsCoordinator _instance;

        private readonly IReadOnlyRuntimeModel _runtimeModel;
        private readonly TimeUtil _timeUtil;
        private float _timeUntilRefresh;

        public static PlayerUnitsCoordinator Instance =>
            _instance ??= new PlayerUnitsCoordinator(
                ServiceLocator.Get<IReadOnlyRuntimeModel>(),
                ServiceLocator.Get<TimeUtil>());

        public IReadOnlyUnit RecommendedTarget { get; private set; }
        public Vector2Int RecommendedPoint { get; private set; }
        public bool HasRecommendedPoint { get; private set; }
        public bool HasEnemiesOnPlayerHalf { get; private set; }

        public static PlayerUnitsCoordinator Create(IReadOnlyRuntimeModel runtimeModel, TimeUtil timeUtil)
        {
            if (_instance != null)
                return _instance;

            _instance = new PlayerUnitsCoordinator(runtimeModel, timeUtil);
            return _instance;
        }

        private PlayerUnitsCoordinator(IReadOnlyRuntimeModel runtimeModel, TimeUtil timeUtil)
        {
            _runtimeModel = runtimeModel;
            _timeUtil = timeUtil;
            _timeUtil.AddFixedUpdateAction(Update);
        }

        public void RefreshRecommendations()
        {
            RecommendedTarget = null;
            HasRecommendedPoint = false;
            HasEnemiesOnPlayerHalf = false;

            if (_runtimeModel.RoMap == null)
                return;

            var enemies = _runtimeModel.RoBotUnits.ToList();
            if (enemies.Count == 0)
                return;

            var enemiesOnPlayerHalf = enemies
                .Where(IsOnPlayerHalf)
                .OrderBy(DistanceToPlayerBaseSqr)
                .ToList();

            HasEnemiesOnPlayerHalf = enemiesOnPlayerHalf.Count > 0;
            RecommendedTarget = HasEnemiesOnPlayerHalf
                ? enemiesOnPlayerHalf[0]
                : enemies
                    .OrderBy(enemy => enemy.Health)
                    .ThenBy(DistanceToPlayerBaseSqr)
                    .First();

            var pointTarget = HasEnemiesOnPlayerHalf
                ? RecommendedTarget
                : enemies.OrderBy(DistanceToPlayerBaseSqr).First();

            RecommendedPoint = HasEnemiesOnPlayerHalf
                ? FindDefencePoint()
                : FindAttackPoint(pointTarget);

            HasRecommendedPoint = true;
        }

        private void Update(float deltaTime)
        {
            _timeUntilRefresh -= deltaTime;
            if (_timeUntilRefresh > 0f)
                return;

            _timeUntilRefresh = RecommendationRefreshDelay;
            RefreshRecommendations();
        }

        private bool IsOnPlayerHalf(IReadOnlyUnit enemy)
        {
            var playerBase = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];
            var enemyBase = _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
            return (enemy.Pos - playerBase).sqrMagnitude <= (enemy.Pos - enemyBase).sqrMagnitude;
        }

        private int DistanceToPlayerBaseSqr(IReadOnlyUnit unit)
        {
            var playerBase = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];
            return (unit.Pos - playerBase).sqrMagnitude;
        }

        private Vector2Int FindDefencePoint()
        {
            var playerBase = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];
            var enemyBase = _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
            var direction = GetStepDirection(playerBase, enemyBase);
            var distanceFromBase = Mathf.Max(1, Mathf.RoundToInt(GetAveragePlayerAttackRange() * 0.5f));

            return FindClosestWalkablePoint(playerBase + direction * distanceFromBase);
        }

        private Vector2Int FindAttackPoint(IReadOnlyUnit target)
        {
            var playerBase = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];
            var directionToBase = ((Vector2)playerBase - target.Pos).normalized;
            if (directionToBase == Vector2.zero)
                directionToBase = Vector2.down;

            var desiredPoint = (Vector2)target.Pos + directionToBase * GetAveragePlayerAttackRange();
            return FindClosestWalkablePoint(Vector2Int.RoundToInt(desiredPoint));
        }

        private float GetAveragePlayerAttackRange()
        {
            return _runtimeModel.RoPlayerUnits
                .Select(playerUnit => playerUnit.Config.AttackRange)
                .DefaultIfEmpty(DefaultAttackRange)
                .Average();
        }

        private Vector2Int FindClosestWalkablePoint(Vector2Int desiredPoint)
        {
            if (IsWalkablePoint(desiredPoint))
                return desiredPoint;

            var maxDistance = Mathf.Max(_runtimeModel.RoMap.Width, _runtimeModel.RoMap.Height);
            for (int radius = 1; radius <= maxDistance; radius++)
            {
                foreach (var point in GetRing(desiredPoint, radius))
                {
                    if (IsWalkablePoint(point))
                        return point;
                }
            }

            return _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];
        }

        private bool IsWalkablePoint(Vector2Int point)
        {
            return !_runtimeModel.RoMap[point] &&
                   _runtimeModel.RoUnits.All(unit => unit.Pos != point);
        }

        private static IEnumerable<Vector2Int> GetRing(Vector2Int center, int radius)
        {
            for (int shift = -radius; shift <= radius; shift++)
            {
                yield return center + new Vector2Int(shift, radius);
                yield return center + new Vector2Int(shift, -radius);
                yield return center + new Vector2Int(radius, shift);
                yield return center + new Vector2Int(-radius, shift);
            }
        }

        private static Vector2Int GetStepDirection(Vector2Int from, Vector2Int to)
        {
            var delta = to - from;
            return new Vector2Int(GetStep(delta.x), GetStep(delta.y));
        }

        private static int GetStep(int value)
        {
            if (value == 0)
                return 0;

            return value > 0 ? 1 : -1;
        }
    }
}
