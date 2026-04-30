using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.ReadOnly;
using UnityEngine;
using Utilities;

namespace UnitBrains
{
    public class ArmyBrain
    {
        private readonly TimeUtil _timeUtil;
        private readonly IReadOnlyRuntimeModel _runtimeModel;

        public ArmyBrain()
        {
            _timeUtil = ServiceLocator.Get<TimeUtil>();
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
        }

        // Ближайший к нашей базе враг, если враги на нашей половине;
        // иначе — враг с наименьшим HP.
        public IReadOnlyUnit GetRecommendedTarget()
        {
            var enemies = _runtimeModel.RoBotUnits.ToList();
            if (!enemies.Any()) return null;

            var onOurHalf = EnemiesOnOurHalf(enemies);
            if (onOurHalf.Any())
                return onOurHalf.OrderBy(e => DistanceToPlayerBase(e.Pos)).First();

            return enemies.OrderBy(e => e.Health).First();
        }

        // Перед нашей базой, если враги на нашей половине;
        // иначе — на расстоянии выстрела от ближайшего к базе врага.
        public Vector2Int GetRecommendedPoint()
        {
            var enemies = _runtimeModel.RoBotUnits.ToList();
            var playerBase = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];

            if (!enemies.Any())
                return playerBase;

            var onOurHalf = EnemiesOnOurHalf(enemies);
            if (onOurHalf.Any())
            {
                var enemyBase = _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
                var step = DirectionStep(playerBase, enemyBase);
                return playerBase + step * 3;
            }

            var nearest = enemies.OrderBy(e => DistanceToPlayerBase(e.Pos)).First();
            return PointAtRangeFrom(nearest.Pos, playerBase, attackRange: 3);
        }

        private List<IReadOnlyUnit> EnemiesOnOurHalf(List<IReadOnlyUnit> enemies)
        {
            var playerBase = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];
            int half = _runtimeModel.RoMap.Width / 2;
            bool baseOnLeft = playerBase.x <= half;
            return baseOnLeft
                ? enemies.Where(e => e.Pos.x <= half).ToList()
                : enemies.Where(e => e.Pos.x > half).ToList();
        }

        private float DistanceToPlayerBase(Vector2Int pos)
        {
            return Vector2Int.Distance(pos, _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]);
        }

        private static Vector2Int DirectionStep(Vector2Int from, Vector2Int to)
        {
            var d = to - from;
            return new Vector2Int(
                d.x == 0 ? 0 : (d.x > 0 ? 1 : -1),
                d.y == 0 ? 0 : (d.y > 0 ? 1 : -1)
            );
        }

        private static Vector2Int PointAtRangeFrom(Vector2Int origin, Vector2Int toward, int attackRange)
        {
            var delta = toward - origin;
            float dist = delta.magnitude;
            if (dist < 1f) return origin;
            return origin + new Vector2Int(
                Mathf.RoundToInt(delta.x / dist * attackRange),
                Mathf.RoundToInt(delta.y / dist * attackRange)
            );
        }
    }
}
