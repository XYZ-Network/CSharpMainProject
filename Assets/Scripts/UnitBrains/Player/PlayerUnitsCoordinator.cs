using System;
using System.Linq;
using Model;
using Model.Runtime.ReadOnly;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    public interface IPlayerUnitsCoordinator
    {
        IReadOnlyUnit RecommendedTargetUnit { get; }
        Vector2Int RecommendedPoint { get; }
    }

    /// <summary>
    /// Координатор юнитов — теперь не синглтон, можно создавать несколько инстансов.
    /// Меняет поведение из оригинального PlayerUnitsCoordinatorSingleton, но принимает
    /// зависимости через конструктор и работает в контексте конкретного игрока.
    /// </summary>
    public sealed class PlayerUnitsCoordinator : IPlayerUnitsCoordinator
    {
        private readonly IReadOnlyRuntimeModel _runtimeModel;
        private readonly TimeUtil _timeUtil;

        private readonly float _updateInterval = 0.5f;
        private float _accumulator = 0f;

        private IReadOnlyUnit _cachedRecommendedUnit;
        private Vector2Int _cachedRecommendedPoint;

        private readonly bool _forPlayer; // true => coordinates for PlayerId, false => for BotPlayerId

        public PlayerUnitsCoordinator(IReadOnlyRuntimeModel runtimeModel, TimeUtil timeUtil, bool forPlayer)
        {
            _runtimeModel = runtimeModel ?? throw new ArgumentNullException(nameof(runtimeModel));
            _timeUtil = timeUtil ?? throw new ArgumentNullException(nameof(timeUtil));
            _forPlayer = forPlayer;

            _timeUtil.AddUpdateAction(OnUpdate);
            Recalculate();
        }

        private void OnUpdate(float delta)
        {
            _accumulator += delta;
            if (_accumulator >= _updateInterval)
            {
                _accumulator = 0f;
                Recalculate();
            }
        }

        private void Recalculate()
        {
            if (_runtimeModel == null)
            {
                _cachedRecommendedUnit = null;
                _cachedRecommendedPoint = Vector2Int.zero;
                return;
            }

            var ourBase = _runtimeModel.RoMap.Bases[_forPlayer ? RuntimeModel.PlayerId : RuntimeModel.BotPlayerId];
            var enemyBase = _runtimeModel.RoMap.Bases[_forPlayer ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];

            // enemies are units that belong to the opposite side
            var enemies = _runtimeModel.RoUnits.Where(u => u.Config.IsPlayerUnit != _forPlayer).ToList();

            if (!enemies.Any())
            {
                _cachedRecommendedUnit = null;
                _cachedRecommendedPoint = enemyBase;
                return;
            }

            var enemiesOnOurHalf = enemies
                .Where(e => Vector2Int.Distance(e.Pos, ourBase) < Vector2Int.Distance(e.Pos, enemyBase))
                .ToList();

            if (enemiesOnOurHalf.Any())
            {
                _cachedRecommendedUnit = enemiesOnOurHalf
                    .OrderBy(e => Vector2Int.Distance(e.Pos, ourBase))
                    .First();

                var dirX = Math.Sign(enemyBase.x - ourBase.x);
                var dirY = Math.Sign(enemyBase.y - ourBase.y);
                var recommended = ourBase + new Vector2Int(dirX, dirY);
                _cachedRecommendedPoint = recommended;
                return;
            }

            _cachedRecommendedUnit = enemies.OrderBy(e => e.Health).First();

            var nearestToBase = enemies.OrderBy(e => Vector2Int.Distance(e.Pos, ourBase)).First();

            float defaultAttackRange = 3f;
            float minAttackRange = defaultAttackRange;
            try
            {
                var playerUnits = _runtimeModel.RoPlayerUnits.ToList();
                if (playerUnits.Any())
                    minAttackRange = playerUnits.Min(u => u.Config.AttackRange);
            }
            catch
            {
                minAttackRange = defaultAttackRange;
            }

            var dirToOurBase = (Vector2)(ourBase - nearestToBase.Pos);
            var len = dirToOurBase.magnitude;
            if (len <= 0.0001f)
            {
                _cachedRecommendedPoint = nearestToBase.Pos;
            }
            else
            {
                var pointF = (Vector2)nearestToBase.Pos + (dirToOurBase / len) * minAttackRange;
                _cachedRecommendedPoint = new Vector2Int(Mathf.RoundToInt(pointF.x), Mathf.RoundToInt(pointF.y));
            }
        }

        public IReadOnlyUnit RecommendedTargetUnit => _cachedRecommendedUnit;
        public Vector2Int RecommendedPoint => _cachedRecommendedPoint;
    }
}