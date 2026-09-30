using System.Linq;
using Model;
using Model.Runtime.ReadOnly;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    /// <summary>
    /// Синглтон-координатор юнитов игрока.
    /// Подписывается на TimeUtil (FixedUpdate) и раз в UpdateInterval секунд
    /// пересчитывает рекомендации:
    ///   - RecommendedTarget — кого атаковать;
    ///   - RecommendedPoint  — куда стягиваться.
    /// </summary>
    public class PlayerUnitCoordinator
    {
        private static PlayerUnitCoordinator _instance;

        public static PlayerUnitCoordinator Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PlayerUnitCoordinator();
                    _instance.Subscribe();
                }
                return _instance;
            }
        }

        public Vector2Int? RecommendedTarget { get; private set; }
        public Vector2Int? RecommendedPoint { get; private set; }

        private const float UpdateInterval = 0.25f;

        private float _accumulatedTime = 0f;
        private float _nextUpdateTime = 0f;
        private bool _subscribed = false;

        private PlayerUnitCoordinator() { }

        /// <summary>
        /// Подписка на TimeUtil. Вызывается один раз при первом обращении к Instance.
        /// </summary>
        private void Subscribe()
        {
            if (_subscribed) return;
            if (!ServiceLocator.Contains<TimeUtil>()) return;

            var timeUtil = ServiceLocator.Get<TimeUtil>();
            timeUtil.AddFixedUpdateAction(OnFixedUpdate);
            _subscribed = true;
        }

        /// <summary>
        /// Отписка — на всякий случай, если координатор когда-нибудь
        /// понадобится сбросить (например, между уровнями).
        /// </summary>
        public void Unsubscribe()
        {
            if (!_subscribed) return;
            if (!ServiceLocator.Contains<TimeUtil>()) return;

            var timeUtil = ServiceLocator.Get<TimeUtil>();
            timeUtil.RemoveFixedUpdateAction(OnFixedUpdate);
            _subscribed = false;
        }

        private void OnFixedUpdate(float deltaTime)
        {
            _accumulatedTime += deltaTime;
            if (_accumulatedTime < _nextUpdateTime)
                return;

            _nextUpdateTime = _accumulatedTime + UpdateInterval;
            Recalculate();
        }

        /// <summary>
        /// Тяжёлая часть — пересчёт рекомендаций.
        /// </summary>
        private void Recalculate()
        {
            var model = ServiceLocator.Get<IReadOnlyRuntimeModel>();
            if (model == null)
                return;

            Vector2Int ownBase = model.RoMap.Bases[RuntimeModel.PlayerId];
            Vector2Int enemyBase = model.RoMap.Bases[RuntimeModel.BotPlayerId];

            var enemyUnits = model.RoUnits
                .Where(u => !u.Config.IsPlayerUnit)
                .ToList();

            // Позиции врагов, по которым координатор ориентируется.
            // Если живых юнитов нет — считаем таковой базу противника.
            var enemyPositions = enemyUnits.Select(u => u.Pos).ToList();
            if (enemyPositions.Count == 0)
                enemyPositions.Add(enemyBase);

            // «Наша половина» — та, где расстояние до нашей базы <= чем до вражеской.
            bool OnOurHalf(Vector2Int p) =>
                Vector2Int.Distance(p, ownBase) <= Vector2Int.Distance(p, enemyBase);

            var enemiesOnOurHalf = enemyPositions.Where(OnOurHalf).ToList();

            if (enemiesOnOurHalf.Count > 0)
            {
                // Кейс A: враг у нас дома.
                var closest = enemiesOnOurHalf
                    .OrderBy(p => Vector2Int.Distance(p, ownBase))
                    .First();

                RecommendedTarget = closest;
                RecommendedPoint = ownBase + StepTowards(ownBase, enemyBase);
            }
            else
            {
                // Кейс B: врагов на нашей половине нет.
                // Всегда выбираем ближайшего к нашей базе — это и приказ, и самая опасная цель.
                var closestToBase = enemyPositions
                    .OrderBy(p => Vector2Int.Distance(p, ownBase))
                    .First();

                RecommendedTarget = closestToBase;

                // Точка сбора — на минимальный радиус атаки от ближайшего врага.
                var players = model.RoUnits.Where(u => u.Config.IsPlayerUnit).ToList();
                int range = players.Count > 0
                    ? Mathf.Max(1, Mathf.RoundToInt(players.Min(u => u.Config.AttackRange)))
                    : 1;

                var dir = Sign(ownBase - closestToBase);
                RecommendedPoint = closestToBase + dir * range;
            }       

        }

        /// <summary>
        /// Единичный шаг по 8 направлениям из from в сторону to.
        /// </summary>
        private static Vector2Int StepTowards(Vector2Int from, Vector2Int to)
        {
            var d = to - from;
            return new Vector2Int(
                d.x == 0 ? 0 : (d.x > 0 ? 1 : -1),
                d.y == 0 ? 0 : (d.y > 0 ? 1 : -1));
        }

        private static Vector2Int Sign(Vector2Int v)
        {
            return new Vector2Int(
                v.x == 0 ? 0 : (v.x > 0 ? 1 : -1),
                v.y == 0 ? 0 : (v.y > 0 ? 1 : -1));
        }
    }
}