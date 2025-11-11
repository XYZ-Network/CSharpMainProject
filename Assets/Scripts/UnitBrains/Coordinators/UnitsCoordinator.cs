using System.Linq;
using Model;
using Model.Runtime;
using Model.Runtime.ReadOnly;
using UnityEngine;
using Utilities;

namespace UnitBrains.Coordinators
{
    public class UnitsCoordinator
    {
        private Vector2Int? RecommendedTarget { get; set; }
        private Vector2Int RecommendedPosition { get; set; }

        private readonly IReadOnlyRuntimeModel _runtimeModel;
        private readonly TimeUtil _timeUtil;
        private bool _isEnemyUnit;
        public UnitsCoordinator(
            IReadOnlyRuntimeModel runtimeModel,
            TimeUtil timeUtil,
            bool isEnemyUnit
            )
        {
            _runtimeModel = runtimeModel;
            _timeUtil = timeUtil;
            _timeUtil.AddFixedUpdateAction(Update);
            _isEnemyUnit = isEnemyUnit;
        }

        ~UnitsCoordinator()
        {
            _timeUtil.RemoveFixedUpdateAction(Update);
        }

        public Vector2Int? GetRecomendedTarget(bool isEnemyUnit)
        {
            return this.RecommendedTarget;
        }

        public Vector2Int GetRecomendedPosition(bool isEnemyUnit)
        {
            return this.RecommendedPosition;
        }

        private void Update(float deltaTime)
        {
            var ourBase = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];
            var enemies = _runtimeModel.RoBotUnits;
            var enemyBase = _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];

            if (!_isEnemyUnit)
            {
                var enemiesOnOurHalf = enemies.Where(e => IsOnOurHalf(e.Pos)).ToList();


                if (enemiesOnOurHalf.Any())
                {
                    RecommendedTarget = enemiesOnOurHalf.OrderBy(e => Vector2Int.Distance(e.Pos, ourBase)).First().Pos;
                    RecommendedPosition = ourBase + new Vector2Int(0, 5);
                }
                else if (enemies.Any())
                {
                    RecommendedTarget = enemies.OrderBy(e => e.Health).First().Pos;
                    var closestEnemy = enemies.OrderBy(e => Vector2Int.Distance(e.Pos, ourBase)).First();
                    RecommendedPosition = Vector2Int.RoundToInt(
                        Vector2.MoveTowards(
                            closestEnemy.Pos,
                            ourBase,
                            GetAttackRange(closestEnemy) - 1));
                }
                else
                {
                    RecommendedTarget = enemyBase;
                    RecommendedPosition = Vector2Int.RoundToInt(Vector2.MoveTowards(enemyBase, ourBase, 3f));
                }
            }
            else
            {
                var warriors = _runtimeModel.RoPlayerUnits;
                var wariorsOnEnemyHalf = warriors.Where(e => !IsOnOurHalf(e.Pos)).ToList();

                if (wariorsOnEnemyHalf.Any())
                {
                    RecommendedTarget = wariorsOnEnemyHalf.OrderBy(w => Vector2Int.Distance(w.Pos, enemyBase)).First().Pos;
                    RecommendedPosition = enemyBase + new Vector2Int(0, -5);
                }
                else if (warriors.Any())
                {
                    RecommendedTarget = warriors.OrderBy(w => w.Health).First().Pos;
                    var closestWarrior = warriors.OrderBy(w => Vector2.Distance(w.Pos, enemyBase)).First();
                    RecommendedPosition = Vector2Int.RoundToInt(
                        Vector2.MoveTowards(
                            closestWarrior.Pos,
                            enemyBase,
                            GetAttackRange(closestWarrior) - 1));
                }
                else
                {
                    RecommendedTarget = ourBase;
                    RecommendedPosition = Vector2Int.RoundToInt(Vector2.MoveTowards(ourBase, enemyBase, 3f));
                }
            }
        }

        private bool IsOnOurHalf(Vector2Int position)
        {
            var mapWidth = _runtimeModel.RoMap.Width;
            return position.x < mapWidth / 2;
        }

        private static float GetAttackRange(IReadOnlyUnit unit)
        {
            if (unit is IBuffableUnit buffableUnit)
            {
                return buffableUnit.AttackRange;
            }

            return unit.Config.AttackRange;
        }
    }
}
