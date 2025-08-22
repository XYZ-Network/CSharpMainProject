using System.Linq;
using Model;
using Model.Runtime.ReadOnly;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    /// <summary>
    /// Координатор юнитов игрока.
    /// </summary>
    public class PlayerUnitCoordinator
    {
        private static readonly PlayerUnitCoordinator s_instance = new();

        private readonly IReadOnlyRuntimeModel _runtimeModel;
        private readonly int _halfMapWidth;
        private readonly bool _isPlayerOnLeftSide;

        /// <summary>
        /// Текущая основная цель для атаки
        /// </summary>
        public Vector2Int Target { get; private set; }

        /// <summary>
        /// Точка назначения для движения
        /// </summary>
        public Vector2Int Destination { get; private set; }

        public static PlayerUnitCoordinator Instance => s_instance;

        private PlayerUnitCoordinator()
        {
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();

            // Определяем половину карты и сторону игрока
            _halfMapWidth = _runtimeModel.RoMap.Width / 2;
            _isPlayerOnLeftSide = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId].y < _halfMapWidth;

            // Регистрируем метод обновления в игровом цикле
            ServiceLocator.Get<TimeUtil>().AddUpdateAction(Update);
        }

        private void Update(float deltaTime)
        {
            UpdateTarget();
            UpdateDestination();
        }

        /// <summary>
        /// Обновляет целевую позицию для атаки на основе анализа врагов
        /// </summary>
        private void UpdateTarget()
        {
            Target = _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];

            var enemies = _runtimeModel.RoBotUnits.ToList();
            if (!enemies.Any())
                return;

            // враги с наименьшим здоровьем
            enemies.Sort(CompareByHealth);
            Target = enemies.First().Pos;

            // враги на нашей половине карты
            var enemiesOnPlayerSide = enemies.Where(IsTargetOnPlayerSide).ToList();
            if (!enemiesOnPlayerSide.Any())
                return;

            // ближайшие враги к нашей базе
            enemiesOnPlayerSide.Sort(CompareByDistanceToPlayerBase);
            Target = enemiesOnPlayerSide.First().Pos;
        }

        /// <summary>
        /// Обновляет точку назначения для движения юнитов
        /// </summary>
        private void UpdateDestination()
        {
            Destination = _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];

            var enemies = _runtimeModel.RoBotUnits.ToList();
            if (!enemies.Any())
                return;

            // Ближайший враг к нашей базе
            enemies.Sort(CompareByDistanceToPlayerBase);
            Destination = enemies.First().Pos;

            // Если есть враги на нашей половине возвращаем юнитов для защиты базы
            var enemiesOnPlayerSide = enemies.Where(IsTargetOnPlayerSide).ToList();
            if (enemiesOnPlayerSide.Any())
                Destination = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];
        }

        /// <summary>
        /// Сравнивает юнитов по здоровью
        /// </summary>
        private int CompareByHealth(IReadOnlyUnit a, IReadOnlyUnit b) =>
            a.Health.CompareTo(b.Health);

        /// <summary>
        /// Проверяет, находится ли цель на половине карты игрока
        /// </summary>
        private bool IsTargetOnPlayerSide(IReadOnlyUnit target) =>
            (target.Pos.y < _halfMapWidth && _isPlayerOnLeftSide)
            || (target.Pos.y >= _halfMapWidth && !_isPlayerOnLeftSide);

        /// <summary>
        /// Сравнивает юнитов по расстоянию до базы игрока
        /// </summary>
        private int CompareByDistanceToPlayerBase(IReadOnlyUnit a, IReadOnlyUnit b)
        {
            var distanceA = DistanceToPlayerBase(a);
            var distanceB = DistanceToPlayerBase(b);
            return distanceA.CompareTo(distanceB);

            // Локальная функция для вычисления расстояния
            float DistanceToPlayerBase(IReadOnlyUnit target) =>
                Vector2Int.Distance(target.Pos, _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]);
        }
    }
}