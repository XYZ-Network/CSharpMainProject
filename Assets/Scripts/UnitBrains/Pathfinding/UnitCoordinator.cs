using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;

namespace Model.Runtime
{
    public class UnitCoordinator
    {
        private static UnitCoordinator _instance;
        public static UnitCoordinator Instance => _instance ??= new UnitCoordinator();

        private IReadOnlyRuntimeModel _runtimeModel;

        private UnitCoordinator()
        {
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
        }

        public Vector2Int GetRecommendedTarget()
        {
            var enemies = _runtimeModel.RoUnits.Where(u => u.Config.Team != RuntimeModel.PlayerId).ToList();
            if (!enemies.Any())
                return Vector2Int.zero;

            var playerBasePos = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];

            var enemiesOnOurSide = enemies.Where(e => e.Pos.x <= _runtimeModel.RoMap.Width / 2).ToList();
            if (enemiesOnOurSide.Any())
            {
                return enemiesOnOurSide.OrderBy(e => Vector2Int.Distance(e.Pos, playerBasePos)).First().Pos;
            }
            else
            {
                return enemies.OrderBy(e => e.Health).First().Pos;
            }
        }

        public Vector2Int GetRecommendedPosition()
        {
            var enemies = _runtimeModel.RoUnits.Where(u => u.Config.Team != RuntimeModel.PlayerId).ToList();
            if (!enemies.Any())
                return Vector2Int.zero;

            var playerBasePos = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];
            var enemiesOnOurSide = enemies.Where(e => e.Pos.x <= _runtimeModel.RoMap.Width / 2).ToList();

            if (enemiesOnOurSide.Any())
            {
                return new Vector2Int(playerBasePos.x, playerBasePos.y + 1);
            }
            else
            {
                var closestEnemy = enemies.OrderBy(e => Vector2Int.Distance(e.Pos, playerBasePos)).First();
                return new Vector2Int(closestEnemy.Pos.x, closestEnemy.Pos.y - 1);
            }
        }
    }
}

