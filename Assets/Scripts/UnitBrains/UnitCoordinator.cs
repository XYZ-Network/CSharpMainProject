using System.Linq;
using Model;
using Model.Runtime.ReadOnly;
using UnityEngine;
using Utilities;

namespace UnitBrains
{
    public class UnitCoordinator
    {
        private static UnitCoordinator _instance;

        public static UnitCoordinator Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new UnitCoordinator();
                _instance.EnsureTimeUtilRegistered();
                return _instance;
            }
        }

        private readonly IReadOnlyRuntimeModel _runtimeModel;
        private TimeUtil _timeUtil;

        private IReadOnlyUnit _recommendedTarget;
        private Vector2Int _recommendedPosition;

        private const float UpdateInterval = 0.5f;
        private float _updateTimer = UpdateInterval;

        private const float DefaultAttackRange = 3.5f;

        private UnitCoordinator()
        {
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
        }

        // Re-registers with TimeUtil if it was recreated (e.g. new game session)
        private void EnsureTimeUtilRegistered()
        {
            var currentTimeUtil = ServiceLocator.Get<TimeUtil>();
            if (_timeUtil == currentTimeUtil)
                return;

            _timeUtil?.RemoveUpdateAction(OnUpdate);
            _timeUtil = currentTimeUtil;
            _timeUtil.AddUpdateAction(OnUpdate);
        }

        private void OnUpdate(float deltaTime)
        {
            _updateTimer += deltaTime;
            if (_updateTimer < UpdateInterval)
                return;

            _updateTimer = 0f;
            UpdateRecommendations();
        }

        private void UpdateRecommendations()
        {
            var map = _runtimeModel.RoMap;
            if (map?.Bases == null || map.Bases.Count < 2)
                return;

            var playerBasePos = map.Bases[RuntimeModel.PlayerId];
            var botBasePos = map.Bases[RuntimeModel.BotPlayerId];
            var allEnemies = _runtimeModel.RoBotUnits.ToList();

            if (allEnemies.Count == 0)
            {
                _recommendedTarget = null;
                _recommendedPosition = botBasePos;
                return;
            }

            // Enemies are "on our side" if they are closer to our base than to the enemy base
            var enemiesOnOurSide = allEnemies
                .Where(e => Vector2Int.Distance(e.Pos, playerBasePos) <
                            Vector2Int.Distance(e.Pos, botBasePos))
                .ToList();

            if (enemiesOnOurSide.Count > 0)
            {
                // Threat near base: target the enemy closest to our base, defend in front of it
                _recommendedTarget = enemiesOnOurSide
                    .OrderBy(e => Vector2Int.Distance(e.Pos, playerBasePos))
                    .First();
                _recommendedPosition = GetDefensePosition(playerBasePos, botBasePos);
            }
            else
            {
                // No immediate threat: focus on the lowest HP enemy, advance to attack range
                _recommendedTarget = allEnemies.OrderBy(e => e.Health).First();
                var closestEnemy = allEnemies
                    .OrderBy(e => Vector2Int.Distance(e.Pos, playerBasePos))
                    .First();
                _recommendedPosition = GetAttackPosition(closestEnemy.Pos, playerBasePos);
            }
        }

        public IReadOnlyUnit GetRecommendedTarget() => _recommendedTarget;

        public Vector2Int GetRecommendedPosition() => _recommendedPosition;

        // Position a short distance in front of our base, toward the enemy base
        private Vector2Int GetDefensePosition(Vector2Int playerBasePos, Vector2Int botBasePos)
        {
            var dir = (Vector2)(botBasePos - playerBasePos);
            if (dir.sqrMagnitude < 0.01f)
                return playerBasePos;
            dir.Normalize();
            var pos = (Vector2)playerBasePos + dir * (DefaultAttackRange * 0.75f);
            return new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
        }

        // Position at attack range distance from the target, along the axis from our base
        private Vector2Int GetAttackPosition(Vector2Int targetPos, Vector2Int playerBasePos)
        {
            var dir = (Vector2)(targetPos - playerBasePos);
            if (dir.sqrMagnitude < 0.01f)
                return playerBasePos;
            dir.Normalize();
            var pos = (Vector2)targetPos - dir * DefaultAttackRange;
            return new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
        }
    }
}
