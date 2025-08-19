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
        private static UnitCoordinator _instance;
        private IReadOnlyRuntimeModel _runtimeModel;
        private TimeUtil _timeUtil;

        private Vector2Int _recommendedTarget;
        private Vector2Int _recommendedPoint;
        private float _lastUpdateTime;
        private const float UpdateInterval = 0.5f;
        private bool _disposed;

        public static UnitCoordinator Instance => _instance ??= new UnitCoordinator();

        private UnitCoordinator()
        {
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
            _timeUtil = ServiceLocator.Get<TimeUtil>();
            _timeUtil.AddFixedUpdateAction(Update);
            RecalculateRecommendations();
        }
        
        private void Update(float deltaTime)
        {
            if (Time.time - _lastUpdateTime >= UpdateInterval)
            {
                RecalculateRecommendations();
                _lastUpdateTime = Time.time;
            }
        }

        private void RecalculateRecommendations()
        {
            var playerBase = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];
            var botBase = _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];

            var directionToEnemyBase = (UnityEngine.Vector2)(botBase - playerBase);
            directionToEnemyBase.Normalize();

            var enemyUnits = _runtimeModel.RoBotUnits.ToList();
            var enemiesOnPlayerHalf = enemyUnits
                .Where(u => IsOnPlayerHalf(u.Pos, playerBase, botBase))
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
                    : botBase;

                var closestEnemy = enemyUnits.Count > 0
                    ? enemyUnits.OrderBy(u => Vector2Int.Distance(u.Pos, playerBase)).First()
                    : null;

                var enemyPos = closestEnemy?.Pos ?? botBase;
                var directionToPlayerBase = (UnityEngine.Vector2)(playerBase - enemyPos);
                directionToPlayerBase.Normalize();
                _recommendedPoint = enemyPos + Vector2Int.RoundToInt(directionToPlayerBase * 2);
            }
        }

        private bool IsOnPlayerHalf(Vector2Int pos, Vector2Int playerBase, Vector2Int BotBase)
        {
            return (pos.x < BotBase.x && playerBase.x < BotBase.x) ||
                (pos.x > BotBase.x && playerBase.x > BotBase.x);
        }

        public Vector2Int GetRecommendedTarget() => _recommendedTarget;
        public Vector2Int GetRecommendedPoint() => _recommendedPoint;

        public void Dispose()
        {
            if (_disposed) return;
            _timeUtil?.RemoveFixedUpdateAction(Update);
            _disposed = true;
        }

        ~UnitCoordinator() => Dispose();
    }
}
