using System.Linq;
using Model;
using Model.Runtime.ReadOnly;
using UnityEngine;
using Utilities;

namespace UnitBrains.Coordinators
{
    public class UnitsCoordinator
    {
        public static UnitsCoordinator Instance { get; } = new UnitsCoordinator();

        private Vector2Int? PlayerUnitRecommendedTarget { get; set; }
        private Vector2Int? EnemyUnitRecomendedTarget { get; set; }
        private Vector2Int PlayerUnitRecommendedPosition { get; set; }
        private Vector2Int EnemyUnitRecomendedPosition { get; set; }

        private readonly IReadOnlyRuntimeModel _runtimeModel;
        private readonly TimeUtil _timeUtil;

        public Vector2Int? GetRecomendedTarget(bool isEnemyUnit)
        {
            return (isEnemyUnit)
                ? this.EnemyUnitRecomendedTarget
                : this.PlayerUnitRecommendedTarget;
        }

        public Vector2Int GetRecomendedPosition(bool isEnemyUnit)
        {
            return (isEnemyUnit)
                ? this.EnemyUnitRecomendedPosition
                : this.PlayerUnitRecommendedPosition;
        }

        private UnitsCoordinator()
        {
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
            _timeUtil = ServiceLocator.Get<TimeUtil>();
            _timeUtil.AddFixedUpdateAction(Update);
        }

        private void Update(float deltaTime)
        {
            var ourBase = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];
            var enemies = _runtimeModel.RoBotUnits;
            var enemyBase = _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
            var warriors = _runtimeModel.RoPlayerUnits;

            var enemiesOnOurHalf = enemies.Where(e => IsOnOurHalf(e.Pos)).ToList();
            var wariorsOnEnemyHalf = warriors.Where(e => !IsOnOurHalf(e.Pos)).ToList();

            if (enemiesOnOurHalf.Any())
            {
                PlayerUnitRecommendedTarget = enemiesOnOurHalf.OrderBy(e => Vector2Int.Distance(e.Pos, ourBase)).First().Pos;
                PlayerUnitRecommendedPosition = ourBase + new Vector2Int(0, 5);
            }
            else if (enemies.Any())
            {
                PlayerUnitRecommendedTarget = enemies.OrderBy(e => e.Health).First().Pos;
                var closestEnemy = enemies.OrderBy(e => Vector2Int.Distance(e.Pos, ourBase)).First();
                PlayerUnitRecommendedPosition = Vector2Int.RoundToInt(Vector2.MoveTowards(closestEnemy.Pos, ourBase, closestEnemy.Config.AttackRange - 1));
            }
            else
            {
                PlayerUnitRecommendedTarget = enemyBase;
                PlayerUnitRecommendedPosition = Vector2Int.RoundToInt(Vector2.MoveTowards(enemyBase, ourBase, 3f));
            }

            if (wariorsOnEnemyHalf.Any())
            {
                EnemyUnitRecomendedTarget = wariorsOnEnemyHalf.OrderBy(w => Vector2Int.Distance(w.Pos, enemyBase)).First().Pos;
                EnemyUnitRecomendedPosition = enemyBase + new Vector2Int(0, -5);
            }
            else if (warriors.Any())
            {
                EnemyUnitRecomendedTarget = warriors.OrderBy(w => w.Health).First().Pos;
                var closestWarrior = warriors.OrderBy(w => Vector2.Distance(w.Pos, enemyBase)).First();
                EnemyUnitRecomendedPosition = Vector2Int.RoundToInt(Vector2.MoveTowards(closestWarrior.Pos, enemyBase, closestWarrior.Config.AttackRange - 1));
            }
            else
            {
                EnemyUnitRecomendedTarget = ourBase;
                EnemyUnitRecomendedPosition = Vector2Int.RoundToInt(Vector2.MoveTowards(ourBase, enemyBase, 3f));
            }
        }

        private bool IsOnOurHalf(Vector2Int position)
        {
            var mapWidth = _runtimeModel.RoMap.Width;
            return position.x < mapWidth / 2;
        }
    }
}
