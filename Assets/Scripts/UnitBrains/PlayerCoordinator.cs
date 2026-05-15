using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.ReadOnly;
using UnityEngine;
using Utilities;

namespace UnitBrains
{
    public sealed class PlayerCoordinator
    {
        private static PlayerCoordinator _instance;

        public static PlayerCoordinator Instance => _instance ??= new PlayerCoordinator();

        private readonly IReadOnlyRuntimeModel runtimeModel;
        private readonly TimeUtil timeUtil;

        public bool HasRecommendedTarget { get; private set; }
        public Vector2Int RecommendedTarget { get; private set; }
        public Vector2Int RecommendedPoint { get; private set; }

        private PlayerCoordinator()
        {
            runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
            timeUtil = ServiceLocator.Get<TimeUtil>();
        }

        public void Recalculate()
        {
            var myBase = runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];
            var enemyBase = runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];

            var playerUnits = runtimeModel.RoUnits
                .Where(u => u.Config.IsPlayerUnit)
                .ToList();

            var enemyUnits = runtimeModel.RoUnits
                .Where(u => !u.Config.IsPlayerUnit)
                .ToList();

            if (enemyUnits.Count == 0)
            {
                HasRecommendedTarget = false;
                RecommendedTarget = myBase;
                RecommendedPoint = GetNearestWalkable(myBase, myBase);
                return;
            }

            var enemiesOnOurHalf = enemyUnits
                .Where(u => IsOnOurHalf(u.Pos, myBase, enemyBase))
                .ToList();

            if (enemiesOnOurHalf.Count > 0)
            {
                var nearestToBase = enemiesOnOurHalf
                    .OrderBy(u => Manhattan(u.Pos, myBase))
                    .First();

                HasRecommendedTarget = true;
                RecommendedTarget = nearestToBase.Pos;

                Vector2Int frontOfBase = myBase + GetStep(myBase, enemyBase);
                RecommendedPoint = GetNearestWalkable(frontOfBase, myBase);
                return;
            }

            var weakestEnemy = enemyUnits
                .OrderBy(GetUnitHealth)
                .ThenBy(u => Manhattan(u.Pos, myBase))
                .First();

            var nearestEnemyToBase = enemyUnits
                .OrderBy(u => Manhattan(u.Pos, myBase))
                .First();

            HasRecommendedTarget = true;
            RecommendedTarget = weakestEnemy.Pos;

            float minAttackRange = playerUnits.Count > 0
                ? playerUnits.Min(u => u.Config.AttackRange)
                : 1f;

            int rangeCells = Mathf.Max(1, Mathf.RoundToInt(minAttackRange));
            Vector2Int stepToBase = GetStep(nearestEnemyToBase.Pos, myBase);
            Vector2Int desiredPoint = nearestEnemyToBase.Pos + stepToBase * rangeCells;

            RecommendedPoint = GetNearestWalkable(desiredPoint, myBase);
        }
       
           private bool IsOnOurHalf(Vector2Int point, Vector2Int myBase, Vector2Int enemyBase)
        {
            Vector2 center = ((Vector2)myBase + (Vector2)enemyBase) * 0.5f;
            Vector2 toMyBase = (Vector2)myBase - center;
            Vector2 toPoint = (Vector2)point - center;

            return Vector2.Dot(toPoint, toMyBase) >= 0f;
        }
        
        private Vector2Int GetNearestWalkable(Vector2Int desired, Vector2Int fallback)
        {
            if (runtimeModel.IsTileWalkable(desired))
                return desired;

            const int maxRadius = 10;

            for (int radius = 1; radius <= maxRadius; radius++)
            {
                for (int x = desired.x - radius; x <= desired.x + radius; x++)
                {
                    for (int y = desired.y - radius; y <= desired.y + radius; y++)
                    {
                        Vector2Int cell = new Vector2Int(x, y);

                        int chebyshev = Mathf.Max(
                            Mathf.Abs(cell.x - desired.x),
                            Mathf.Abs(cell.y - desired.y));

                        if (chebyshev != radius)
                            continue;

                        if (!runtimeModel.IsTileWalkable(cell))
                            continue;

                        return cell;
                    }
                }
            }

            return fallback;
        }

        private static Vector2Int GetStep(Vector2Int from, Vector2Int to)
        {
            int dx = Mathf.Clamp(to.x - from.x, -1, 1);
            int dy = Mathf.Clamp(to.y - from.y, -1, 1);
            return new Vector2Int(dx, dy);
        }

        private static int Manhattan(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        private static float GetUnitHealth(IReadOnlyUnit unit)
        {
            var type = unit.GetType();

            foreach (string propertyName in new[] { "Health", "Hp", "CurrentHealth" })
            {
                var property = type.GetProperty(propertyName);
                if (property == null)
                    continue;

                object value = property.GetValue(unit);
                if (value != null)
                    return Convert.ToSingle(value);
            }

            return unit.Config.MaxHealth;
        }
    }
}
