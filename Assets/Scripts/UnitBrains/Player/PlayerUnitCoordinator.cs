using System.Collections.Generic; 
using System.Linq; 
using Model; 
using Model.Runtime.ReadOnly; 
using UnityEngine; 
using Utilities; 

namespace UnitBrains.Player 
{
    public class PlayerUnitCoordinator // Объявляем класс координатора юнитов игрока
    {
        private static PlayerUnitCoordinator _instance; // Статическое поле для хранения единственного экземпляра

        private static readonly object _lockObject = new object(); // Объект для потокобезопасной блокировки

        private IReadOnlyRuntimeModel _runtimeModel; // Ссылка на модель игры

        private TimeUtil _timeUtil; // Ссылка на утилиту времени

        private Vector2Int _recommendedTarget; // Рекомендуемая цель для атаки

        private Vector2Int _recommendedPoint; // Рекомендуемая точка для движения

        private float _lastUpdateTime; // Время последнего обновления

        private const float UpdateInterval = 0.5f; // Интервал обновления (0.5 секунды)

        public Vector2Int RecommendedTarget => _recommendedTarget; // Свойство для получения цели

        public Vector2Int RecommendedPoint => _recommendedPoint; // Свойство для получения точки

        public bool HasRecommendations { get; private set; } // Флаг наличия рекомендаций

        private PlayerUnitCoordinator() // Приватный конструктор
        {
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>(); // Получаем модель игры

            _timeUtil = ServiceLocator.Get<TimeUtil>(); // Получаем TimeUtil

            if (_timeUtil != null) // Если TimeUtil доступен
            {
                _timeUtil.AddFixedUpdateAction(UpdateRecommendations); // Подписываемся на обновления
            }

            HasRecommendations = false; // Изначально рекомендаций нет
        }

        public static PlayerUnitCoordinator Instance // Свойство для получения экземпляра
        {
            get // Геттер
            {
                if (_instance == null) // Если экземпляр не создан
                {
                    lock (_lockObject) // Блокируем для потокобезопасности
                    {
                        if (_instance == null) // Повторная проверка
                        {
                            _instance = new PlayerUnitCoordinator(); // Создаем экземпляр
                        }
                    }
                }
                return _instance; // Возвращаем экземпляр
            }
        }

        private void UpdateRecommendations(float deltaTime) // Метод обновления рекомендаций
        {
            if (Time.time - _lastUpdateTime < UpdateInterval) // Если рано обновлять
                return; // Выходим

            _lastUpdateTime = Time.time; // Обновляем время

            CalculateRecommendations(); // Пересчитываем рекомендации
        }

        private void CalculateRecommendations() // Метод расчета рекомендаций
        {
            var playerBase = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]; // База игрока
            var botBase = _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId]; // База бота

            var enemyUnits = _runtimeModel.RoUnits // Все юниты
                .Where(u => u.Config.IsPlayerUnit == false) // Только враги
                .ToList(); // В список

            if (enemyUnits.Count == 0) // Если врагов нет
            {
                HasRecommendations = false; // Сбрасываем флаг
                return; // Выходим
            }

            var enemiesOnOurHalf = enemyUnits // Враги
                .Where(u => IsOnOurHalf(u.Pos, playerBase, botBase)) // На нашей половине
                .ToList(); // В список

            if (enemiesOnOurHalf.Count > 0) // Если есть враги на нашей половине
            {
                _recommendedTarget = GetClosestToBase(enemiesOnOurHalf, playerBase); // Ближайший к базе
                _recommendedPoint = GetPointInFrontOfBase(playerBase, botBase); // Точка перед базой
            }
            else // Если врагов на нашей половине нет
            {
                _recommendedTarget = GetWeakestEnemy(enemyUnits); // Самый слабый враг
                _recommendedPoint = GetPointAtAttackRangeFromEnemy(_recommendedTarget, playerBase); // Точка атаки
            }

            HasRecommendations = true; // Устанавливаем флаг
        }

        private bool IsOnOurHalf(Vector2Int pos, Vector2Int playerBase, Vector2Int botBase) // Проверка половины
        {
            float distToPlayerBase = Vector2Int.Distance(pos, playerBase); // Расстояние до нашей базы
            float distToBotBase = Vector2Int.Distance(pos, botBase); // Расстояние до базы врага
            return distToPlayerBase < distToBotBase; // Наша половина если ближе к нам
        }

        private Vector2Int GetClosestToBase(List<IReadOnlyUnit> enemies, Vector2Int basePos) // Ближайший враг
        {
            return enemies // Враги
                .OrderBy(u => Vector2Int.Distance(u.Pos, basePos)) // Сортируем по расстоянию
                .First() // Первый
                .Pos; // Позиция
        }

        private Vector2Int GetWeakestEnemy(List<IReadOnlyUnit> enemies) // Самый слабый враг
        {
            return enemies // Враги
                .OrderBy(u => u.Health) // Сортируем по здоровью
                .First() // Первый
                .Pos; // Позиция
        }

        private Vector2Int GetPointInFrontOfBase(Vector2Int playerBase, Vector2Int botBase) // Точка перед базой
        {
            Vector2Int direction = new Vector2Int( // Направление
                Mathf.RoundToInt(Mathf.Sign(botBase.x - playerBase.x)), // По X
                Mathf.RoundToInt(Mathf.Sign(botBase.y - playerBase.y)) // По Y
            );

            Vector2Int point = playerBase + direction * 3; // Точка в 3 клетках от базы

            if (IsWalkable(point)) // Если проходима
                return point; // Возвращаем

            return FindNearestWalkablePoint(playerBase, point); // Ищем ближайшую
        }

        private Vector2Int GetPointAtAttackRangeFromEnemy(Vector2Int enemyPos, Vector2Int playerBase) // Точка атаки
        {
            Vector2Int direction = new Vector2Int( // Направление
                Mathf.RoundToInt(Mathf.Sign(playerBase.x - enemyPos.x)), // По X
                Mathf.RoundToInt(Mathf.Sign(playerBase.y - enemyPos.y)) // По Y
            );

            Vector2Int point = enemyPos + direction * 3; // Точка в 3 клетках от врага

            if (IsWalkable(point)) // Если проходима
                return point; // Возвращаем

            return FindNearestWalkablePoint(enemyPos, point); // Ищем ближайшую
        }

        private bool IsWalkable(Vector2Int pos) // Проверка проходимости
        {
            if (pos.x < 0 || pos.x >= _runtimeModel.RoMap.Width || // Границы X
                pos.y < 0 || pos.y >= _runtimeModel.RoMap.Height) // Границы Y
                return false; // Непроходимо

            if (_runtimeModel.RoMap[pos]) // Если стена
                return false; // Непроходимо

            if (_runtimeModel.RoUnits.Any(u => u.Pos == pos)) // Если юнит
                return false; // Непроходимо

            return true; // Проходимо
        }

        private Vector2Int FindNearestWalkablePoint(Vector2Int from, Vector2Int target) // Поиск проходимой точки
        {
            Vector2Int[] directions = new Vector2Int[] // Направления
            {
                Vector2Int.up, // Вверх
                Vector2Int.right, // Вправо
                Vector2Int.down, // Вниз
                Vector2Int.left // Влево
            };

            foreach (Vector2Int dir in directions) // Для каждого направления
            {
                Vector2Int checkPos = target + dir; // Позиция для проверки
                if (IsWalkable(checkPos)) // Если проходима
                    return checkPos; // Возвращаем
            }

            return from; // Возвращаем исходную
        }

        // ИСПРАВЛЕННЫЙ МЕТОД: Убрана проверка типа, теперь только возвращает рекомендации
        public bool ShouldReactToTarget(Vector2Int unitPos, int unitAttackRange) // Метод проверки реакции
        {
            if (!HasRecommendations) // Если нет рекомендаций
                return false; // Не реагируем

            float distance = Vector2Int.Distance(unitPos, _recommendedTarget); // Расстояние до цели (float)
            float attackRange = unitAttackRange; // Конвертируем int в float

            return distance <= attackRange * 2f; // Сравниваем float с float
        }
    }
}