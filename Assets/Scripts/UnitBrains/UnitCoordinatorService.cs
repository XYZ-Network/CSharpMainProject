using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Utilities;

namespace UnitBrains
{
    public class UnitCoordinatorService
    {
        private IReadOnlyRuntimeModel _runtimeModel;
        private TimeUtil _timeUtil;
        private Vector2Int _playerBase => _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];
        private Vector2Int _enemyBase => _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];

        public UnitCoordinatorService(IReadOnlyRuntimeModel runtimeModel, TimeUtil timeUtil)
        {
            _runtimeModel = runtimeModel;
            _timeUtil = timeUtil;
            _timeUtil.AddFixedUpdateAction(UpdateRecomendations);
            IsInitialized = true;
        }

        public bool IsInitialized { get; private set; }

        public Vector2Int RecomendedTarget { get; private set; }
        public Vector2Int RecomendedPoint { get; private set; }

        private int MapMiddleX => _runtimeModel.RoMap.Width / 2;

        private void UpdateRecomendations(float deltaTime)
        {
            if (!IsInitialized || _runtimeModel?.RoMap?.Bases == null) return;
            NewRecomendedTarget();
            NewRecomendedPoint();
        }

        private void NewRecomendedTarget()
        {
            var enemies = _runtimeModel.RoBotUnits.ToList();

            if (enemies.Count == 0)
            {
                RecomendedTarget = _enemyBase;
                return;
            }

            var enemiesOnPlayerHalf = enemies.Where(u => u.Pos.x < MapMiddleX).ToList();
            if (enemiesOnPlayerHalf.Count > 0)
            {
                RecomendedTarget = enemiesOnPlayerHalf.OrderBy(u => Vector2Int.Distance(u.Pos, _playerBase)).First().Pos;
            }
            else
            {
                RecomendedTarget = enemies.OrderBy(u => u.Health).First().Pos;
            }
        }

        private void NewRecomendedPoint()
        {
            var enemies = _runtimeModel.RoBotUnits.ToList();

            if (enemies.Count == 0)
            {
                RecomendedPoint = _playerBase;
                return;
            }

            var enemiesOnPlayerHalf = enemies.Where(u => u.Pos.x < MapMiddleX).ToList();
            if (enemiesOnPlayerHalf.Count > 0) RecomendedPoint = GetDefensivePosition();
            else
            {
                var closedEnemyToBase = enemies.OrderBy(u => Vector2Int.Distance(u.Pos, _playerBase)).First();
                RecomendedPoint = GetAttackPositionFromEnemy(closedEnemyToBase.Pos);
            }
        }

        private Vector2Int GetDefensivePosition()
        {
            var directionToEnemy = (Vector2)(_enemyBase - _playerBase);
            directionToEnemy.Normalize();
            return _playerBase + Vector2Int.RoundToInt(directionToEnemy * 2f);
        }

        private Vector2Int GetAttackPositionFromEnemy(Vector2Int enemyPos)
        {
            var directionToBase = (Vector2)(_playerBase - enemyPos);
            directionToBase.Normalize();
            var attackRange = 3f;

            return enemyPos + Vector2Int.RoundToInt(directionToBase * attackRange);
        }

        public bool IsTargetInRange(Vector2Int unitPos, Vector2Int targetPos, float attackRange)
        {
            float distangeSqr = (targetPos - unitPos).sqrMagnitude;
            return distangeSqr <= (attackRange * 2) * (attackRange * 2);
        }

        public void Dispose()
        {
            _timeUtil?.RemoveFixedUpdateAction(UpdateRecomendations);
        }
    }
}