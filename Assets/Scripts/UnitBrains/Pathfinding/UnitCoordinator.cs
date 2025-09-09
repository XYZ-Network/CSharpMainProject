using Model;
using Model.Runtime.ReadOnly;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Utilities;

namespace Assets.Scripts.UnitBrains
{
    public sealed class UnitCoordinator
    {        
        private IReadOnlyRuntimeModel _runtimeModel;
        private TimeUtil _timeUtil;
        private readonly bool _isPlayerCoordinator;

        private Vector2Int _recommendedTarget;
        private Vector2Int _recommendedPoint;
        private float _lastUpdateTime;
        private const float UpdateInterval = 0.5f;
        private bool _disposed;
        private bool _isInitialized;

        public UnitCoordinator(bool isPlayerCoordinator)
        {
            _isPlayerCoordinator = isPlayerCoordinator;
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
            _timeUtil = ServiceLocator.Get<TimeUtil>();
            _timeUtil.AddFixedUpdateAction(Update);

            _recommendedTarget = Vector2Int.zero;
            _recommendedPoint = Vector2Int.zero;
        }
                
        private void Update(float deltaTime)
        {
            if (_runtimeModel.RoMap != null && _runtimeModel.RoMap.Bases != null)
            {
                _isInitialized = true;
            }
            if (_isInitialized && Time.time - _lastUpdateTime >= UpdateInterval)
            {
                RecalculateRecommendations();
                _lastUpdateTime = Time.time;
            }
        }

        private void RecalculateRecommendations()
        {
            if (!_isInitialized || _runtimeModel.RoMap == null)
                return;
            try
            {
                var playerId = _isPlayerCoordinator ? RuntimeModel.PlayerId : RuntimeModel.BotPlayerId;
                var enemyId = _isPlayerCoordinator ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId;

                var playerBase = _runtimeModel.RoMap.Bases[playerId];
                var enemyBase = _runtimeModel.RoMap.Bases[enemyId];

                var directionToEnemyBase = (UnityEngine.Vector2)(enemyBase - playerBase);
                directionToEnemyBase.Normalize();

                var enemyUnits = (_isPlayerCoordinator ? _runtimeModel.RoBotUnits : _runtimeModel.RoPlayerUnits).ToList();

                var enemiesOnPlayerHalf = enemyUnits
                    .Where(u => IsOnPlayerHalf(u.Pos, playerBase, enemyBase))
                    .ToList();

                if (enemiesOnPlayerHalf.Count > 0)
                {
                    _recommendedTarget = enemiesOnPlayerHalf
                        .OrderBy(u => Vector2Int.Distance(u.Pos, playerBase))
                        .First().Pos;

                    _recommendedPoint = playerBase + Vector2Int.RoundToInt(directionToEnemyBase * 2);
                }
                else
                {
                    _recommendedTarget = enemyUnits.Count > 0
                        ? enemyUnits.OrderBy(u => u.Health).First().Pos
                        : enemyBase;

                    var closestEnemy = enemyUnits.Count > 0
                        ? enemyUnits.OrderBy(u => Vector2Int.Distance(u.Pos, playerBase)).First()
                        : null;

                    var enemyPos = closestEnemy?.Pos ?? enemyBase;
                    var directionToPlayerBase = (UnityEngine.Vector2)(playerBase - enemyPos);
                    directionToPlayerBase.Normalize();
                    _recommendedPoint = enemyPos + Vector2Int.RoundToInt(directionToPlayerBase * 2);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to calculate recommendation for {(_isPlayerCoordinator ? "player" : "bot")}: {ex.Message}");
                _recommendedTarget = Vector2Int.zero;
                _recommendedPoint = Vector2Int.zero;
            }
        }

        private bool IsOnPlayerHalf(Vector2Int pos, Vector2Int playerBase, Vector2Int enemyBase)
        {
            return (pos.x < enemyBase.x && playerBase.x < enemyBase.x) ||
                (pos.x > enemyBase.x && playerBase.x > enemyBase.x);
        }

        public Vector2Int GetRecommendedTarget() => _recommendedTarget;
        public Vector2Int GetRecommendedPoint() => _recommendedPoint;

        public void Dispose()
        {
            if (_disposed) return;
            _timeUtil?.RemoveFixedUpdateAction(Update);
            _disposed = true;
        }
    }
}
