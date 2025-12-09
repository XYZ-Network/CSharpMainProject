using System;
using System.Linq;
using Model;
using Model.Runtime.ReadOnly;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    /// <summary>
    /// ѕростой синглтон-координатор рекомендаций дл€ юнитов игрока.
    /// ƒоступ к IReadOnlyRuntimeModel и TimeUtil через ServiceLocator.
    ///  эширует рекомендации и обновл€ет их периодически через TimeUtil.
    /// </summary>
    public sealed class PlayerUnitsCoordinatorSingleton
    {
        private static readonly Lazy<PlayerUnitsCoordinatorSingleton> _instance =
            new Lazy<PlayerUnitsCoordinatorSingleton>(() => new PlayerUnitsCoordinatorSingleton());

        public static PlayerUnitsCoordinatorSingleton Instance => _instance.Value;

        private readonly IReadOnlyRuntimeModel _runtimeModel;
        private readonly TimeUtil _timeUtil;

        private readonly float _updateInterval = 0.5f;
        private float _accumulator = 0f;

        private IReadOnlyUnit _cachedRecommendedUnit;
        private Vector2Int _cachedRecommendedPoint;

        private PlayerUnitsCoordinatorSingleton()
        {
            // ѕолучаем сервисы Ч если их нет, оставл€ем ссылки null и не подписываемс€
            if (ServiceLocator.Contains<IReadOnlyRuntimeModel>())
                _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();

            if (ServiceLocator.Contains<TimeUtil>())
            {
                _timeUtil = ServiceLocator.Get<TimeUtil>();
                _timeUtil.AddUpdateAction(OnUpdate);
            }

            // »нициалный расчЄт, если модель доступна
            Recalculate();
        }

        private void OnUpdate(float delta)
        {
            _accumulator += delta;
            if (_accumulator >= _updateInterval)
            {
                _accumulator = 0f;
                Recalculate();
            }
        }

        private void Recalculate()
        {
            if (_runtimeModel == null)
            {
                _cachedRecommendedUnit = null;
                _cachedRecommendedPoint = Vector2Int.zero;
                return;
            }

            // —писок врагов (не наши)
            var enemies = _runtimeModel.RoUnits.Where(u => !u.Config.IsPlayerUnit).ToList();
            var ourBase = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];
            var enemyBase = _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];

            if (!enemies.Any())
            {
                _cachedRecommendedUnit = null;
                // если врагов нет Ч рекомендуем идти к вражеской базе
                _cachedRecommendedPoint = enemyBase;
                return;
            }

            // ¬раги на нашей "половине" Ч ближе к нашей базе, чем к вражеской
            var enemiesOnOurHalf = enemies
                .Where(e => Vector2Int.Distance(e.Pos, ourBase) < Vector2Int.Distance(e.Pos, enemyBase))
                .ToList();

            if (enemiesOnOurHalf.Any())
            {
                // рекомендованна€ цель = ближайший к нашей базе враг на нашей половине
                _cachedRecommendedUnit = enemiesOnOurHalf
                    .OrderBy(e => Vector2Int.Distance(e.Pos, ourBase))
                    .First();

                // рекомендованна€ точка Ч "перед базой": один шаг от базы в сторону вражеской базы
                var dirX = Math.Sign(enemyBase.x - ourBase.x);
                var dirY = Math.Sign(enemyBase.y - ourBase.y);
                var recommended = ourBase + new Vector2Int(dirX, dirY);
                _cachedRecommendedPoint = recommended;
                return;
            }

            // »наче: цель = враг с наименьшим здоровьем
            _cachedRecommendedUnit = enemies.OrderBy(e => e.Health).First();

            // –екомендуема€ точка: на рассто€нии выстрела от ближайшего к базе врага (в сторону нашей базы)
            var nearestToBase = enemies.OrderBy(e => Vector2Int.Distance(e.Pos, ourBase)).First();

            // Ќаходим минимальный радиус атаки среди наших юнитов (если есть) Ч иначе берЄм 3
            float defaultAttackRange = 3f;
            float minAttackRange = defaultAttackRange;
            try
            {
                var playerUnits = _runtimeModel.RoPlayerUnits.ToList();
                if (playerUnits.Any())
                    minAttackRange = playerUnits.Min(u => u.Config.AttackRange);
            }
            catch
            {
                minAttackRange = defaultAttackRange;
            }

            // convert to Vector2 for proper normalization and arithmetic
            var dirToOurBase = (Vector2)(ourBase - nearestToBase.Pos);
            var len = dirToOurBase.magnitude;
            if (len <= 0.0001f)
            {
                _cachedRecommendedPoint = nearestToBase.Pos;
            }
            else
            {
                var pointF = (Vector2)nearestToBase.Pos + (dirToOurBase / len) * minAttackRange;
                _cachedRecommendedPoint = new Vector2Int(Mathf.RoundToInt(pointF.x), Mathf.RoundToInt(pointF.y));
            }
        }

        // ѕубличные свойства Ч дают кэшированную рекомендацию
        public IReadOnlyUnit RecommendedTargetUnit => _cachedRecommendedUnit;
        public Vector2Int RecommendedPoint => _cachedRecommendedPoint;
    }
}