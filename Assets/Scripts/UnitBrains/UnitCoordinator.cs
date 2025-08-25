using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.ReadOnly;
using UnityEngine;
using Utilities;

namespace UnitBrains.Coordinator
{
    public class UnitCoordinator
    {
        public Vector2Int? RecommendedTarget { get; private set; }
        public Vector2Int? RecommendedPoint { get; private set; }

        private readonly IReadOnlyRuntimeModel _runtimeModel;
        private readonly TimeUtil _timeUtil;
        private readonly int _playerId;
        private Vector2Int _playerBasePos;
        private Vector2Int _enemyBasePos;

        public UnitCoordinator(IReadOnlyRuntimeModel runtimeModel, TimeUtil timeUtil, int playerId)
        {
            _runtimeModel = runtimeModel;
            _timeUtil = timeUtil;
            _playerId = playerId;
            Initialize();
        }

        private void Initialize()
        {
            _playerBasePos = _runtimeModel.RoMap.Bases[_playerId];
            _enemyBasePos = _runtimeModel.RoMap.Bases[_playerId == RuntimeModel.PlayerId
                ? RuntimeModel.BotPlayerId
                : RuntimeModel.PlayerId];

            _timeUtil.AddUpdateAction(Update);
        }

        public void Update(float deltaTime)
        {
            var enemyUnits = GetAllEnemyUnits().ToList();
            var enemyBase = _runtimeModel.RoUnits.FirstOrDefault(u =>
                u.Pos == _enemyBasePos && u.Config.IsPlayerUnit != (_playerId == RuntimeModel.PlayerId));

            if (enemyUnits.Any())
            {
                bool enemiesOnOurHalf = enemyUnits.Any(IsOnOurHalf);

                RecommendedTarget = enemiesOnOurHalf
                    ? GetEnemyClosestToBase(enemyUnits)
                    : GetWeakestEnemy(enemyUnits);

                RecommendedPoint = enemiesOnOurHalf
                    ? GetDefensivePosition()
                    : GetOffensivePosition();
            }
            else if (enemyBase != null && enemyBase.Health > 0)
            {
                RecommendedTarget = _enemyBasePos;
                RecommendedPoint = _enemyBasePos;
            }
            else
            {
                RecommendedTarget = null;
                RecommendedPoint = null;
            }
        }

        private Vector2Int GetDefensivePosition()
        {
            return _playerBasePos + new Vector2Int(0, -2);
        }

        private Vector2Int GetOffensivePosition()
        {
            var closestEnemy = GetEnemyClosestToBase(GetAllEnemyUnits().ToList());
            if (!closestEnemy.HasValue)
                return GetDefensivePosition();

            var enemyPos = closestEnemy.Value;
            var direction = (new Vector2(_playerBasePos.x, _playerBasePos.y) -
                          new Vector2(enemyPos.x, enemyPos.y)).normalized;
            var directionInt = new Vector2Int(
                Mathf.RoundToInt(direction.x),
                Mathf.RoundToInt(direction.y));
            return enemyPos + directionInt * 3;
        }

        private Vector2Int? GetEnemyClosestToBase(List<IReadOnlyUnit> enemies)
        {
            return enemies.Count == 0 ? null : enemies
                .OrderBy(u => Vector2Int.Distance(u.Pos, _playerBasePos))
                .First()
                .Pos;
        }

        private Vector2Int? GetWeakestEnemy(List<IReadOnlyUnit> enemies)
        {
            return enemies.Count == 0 ? null : enemies
                .OrderBy(u => u.Health)
                .ThenBy(u => Vector2Int.Distance(u.Pos, _playerBasePos))
                .First()
                .Pos;
        }

        private bool IsOnOurHalf(IReadOnlyUnit unit)
        {
            return unit.Pos.y < _runtimeModel.RoMap.Height / 2;
        }

        private IEnumerable<IReadOnlyUnit> GetAllEnemyUnits()
        {
            return _runtimeModel.RoUnits.Where(u =>
                u.Config.IsPlayerUnit != (_playerId == RuntimeModel.PlayerId) &&
                u.Pos != _enemyBasePos &&
                u.Health > 0);
        }
    }
}