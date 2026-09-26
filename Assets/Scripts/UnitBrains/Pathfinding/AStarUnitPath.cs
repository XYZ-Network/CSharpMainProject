using System.Collections.Generic;
using System.Linq;
using Model;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    public class AStarUnitPath : BaseUnitPath
    {
        // Внутренний класс для хранения информации об узле в алгоритме A*
        private class PathNode
        {
            public Vector2Int Position { get; set; } // Позиция узла на карте
            public float GCost { get; set; } // Стоимость пути от начала до этого узла
            public float HCost { get; set; } // Эвристическая оценка расстояния до цели
            public float FCost => GCost + HCost; // Общая стоимость узла (F = G + H)
            public PathNode Parent { get; set; } // Родительский узел для восстановления пути

            // Конструктор узла - инициализирует начальные значения
            public PathNode(Vector2Int position)
            {
                Position = position; // Устанавливаем позицию узла
                GCost = float.MaxValue; // Начальная стоимость пути - максимальная
                HCost = 0; // Начальная эвристическая оценка - 0
                Parent = null; // Изначально родитель отсутствует
            }
        }

        // Конструктор класса - вызывает конструктор базового класса BaseUnitPath
        public AStarUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint)
            : base(runtimeModel, startPoint, endPoint)
        {
            // Все параметры передаются в базовый конструктор
        }

        // Основной метод расчета пути с использованием алгоритма A*
        protected override void Calculate()
        {
            // Открытый список - содержит узлы, которые предстоит проверить
            List<PathNode> openList = new List<PathNode>();
            // Закрытый список - содержит уже проверенные узлы (хэш-сет для быстрого поиска)
            HashSet<Vector2Int> closedList = new HashSet<Vector2Int>();

            // Создаем начальный узел (позиция юнита)
            PathNode startNode = new PathNode(startPoint);
            startNode.GCost = 0; // Стоимость пути от начала до начала равна 0
            startNode.HCost = CalculateHeuristic(startPoint, endPoint); // Вычисляем эвристику до цели

            // Добавляем начальный узел в открытый список для обработки
            openList.Add(startNode);

            // Основной цикл алгоритма A* - продолжается пока есть узлы для проверки
            while (openList.Count > 0)
            {
                // Находим узел с наименьшей общей стоимостью F = G + H
                PathNode currentNode = openList.OrderBy(node => node.FCost).First();

                // Проверяем, достигли ли мы конечной точки
                if (currentNode.Position == endPoint)
                {
                    // Восстанавливаем полный путь от конечного узла к начальному
                    path = ReconstructPath(currentNode);
                    return; // Завершаем вычисление пути
                }

                // Перемещаем текущий узел из открытого списка в закрытый
                openList.Remove(currentNode);
                closedList.Add(currentNode.Position);

                // Проверяем всех соседей текущего узла (4 направления)
                foreach (Vector2Int neighborPos in GetNeighbors(currentNode.Position))
                {
                    // Пропускаем узлы, которые уже были проверены
                    if (closedList.Contains(neighborPos))
                        continue;

                    // Пропускаем непроходимые клетки (стены или занятые юнитами)
                    if (!IsWalkable(neighborPos))
                        continue;

                    // Вычисляем предварительную стоимость пути до соседа
                    // Текущая стоимость + 1 (стоимость одного шага)
                    float tentativeGCost = currentNode.GCost + 1;

                    // Ищем соседний узел в открытом списке
                    PathNode neighborNode = openList.FirstOrDefault(node => node.Position == neighborPos);

                    if (neighborNode == null) // Если сосед еще не в открытом списке
                    {
                        // Создаем новый узел для соседа
                        neighborNode = new PathNode(neighborPos);
                        neighborNode.GCost = tentativeGCost; // Устанавливаем стоимость пути
                        neighborNode.HCost = CalculateHeuristic(neighborPos, endPoint); // Вычисляем эвристику
                        neighborNode.Parent = currentNode; // Устанавливаем родителя для восстановления пути
                        openList.Add(neighborNode); // Добавляем соседа в открытый список
                    }
                    else if (tentativeGCost < neighborNode.GCost) // Если нашли более короткий путь к соседу
                    {
                        // Обновляем стоимость и родителя (более оптимальный маршрут)
                        neighborNode.GCost = tentativeGCost;
                        neighborNode.Parent = currentNode;
                    }
                }
            }

            // Если цикл завершился, а путь не найден - выводим предупреждение
            Debug.LogWarning($"A* path not found from {startPoint} to {endPoint}, using direct path as fallback");
            // Создаем прямой путь как запасной вариант (от начала к концу)
            path = new Vector2Int[] { startPoint, endPoint };
        }

        // Переопределенный метод получения следующего шага с проверкой валидности
        public new Vector2Int GetNextStepFrom(Vector2Int unitPos)
        {
            // Получаем все точки вычисленного пути в виде списка
            List<Vector2Int> pathPoints = GetPath().ToList();

            // Если путь пустой, возвращаем текущую позицию (некуда идти)
            if (pathPoints.Count == 0)
            {
                Debug.LogWarning($"A* path is empty for unit at {unitPos}");
                return unitPos;
            }

            // Если путь состоит из одной точки, значит мы уже на месте
            if (pathPoints.Count == 1)
            {
                return pathPoints[0];
            }

            // Ищем индекс текущей позиции юнита в вычисленном пути
            int currentIndex = pathPoints.IndexOf(unitPos);

            // Если текущая позиция не найдена в пути (юнит отклонился от маршрута)
            if (currentIndex == -1)
            {
                // Ищем ближайшую точку пути к текущей позиции юнита
                int closestIndex = 0; // Индекс ближайшей найденной точки
                float closestDistance = float.MaxValue; // Минимальное найденное расстояние

                // Проходим по всем точкам пути для поиска ближайшей
                for (int i = 0; i < pathPoints.Count; i++)
                {
                    // Вычисляем евклидово расстояние от текущей позиции до точки пути
                    float distance = Vector2.Distance(
                        new Vector2(unitPos.x, unitPos.y),
                        new Vector2(pathPoints[i].x, pathPoints[i].y)
                    );

                    // Если нашли более близкую точку, обновляем минимум
                    if (distance < closestDistance)
                    {
                        closestDistance = distance; // Сохраняем новое минимальное расстояние
                        closestIndex = i; // Запоминаем индекс ближайшей точки
                    }
                }

                // Если мы не в конце пути, возвращаем следующую точку после ближайшей
                if (closestIndex < pathPoints.Count - 1)
                {
                    Vector2Int nextPoint = pathPoints[closestIndex + 1];
                    // Проверяем и возвращаем валидный шаг к следующей точке
                    return ValidateAndGetStep(unitPos, nextPoint);
                }
                else
                {
                    // Если мы в конце пути, возвращаем последнюю точку (финальная позиция)
                    return ValidateAndGetStep(unitPos, pathPoints[pathPoints.Count - 1]);
                }
            }

            // Если мы уже в конце пути, возвращаем конечную точку
            if (currentIndex >= pathPoints.Count - 1)
            {
                return pathPoints[pathPoints.Count - 1];
            }

            // Получаем следующую точку пути после текущей позиции
            Vector2Int nextPathPoint = pathPoints[currentIndex + 1];

            // Проверяем валидность шага и возвращаем его
            return ValidateAndGetStep(unitPos, nextPathPoint);
        }

        // Вспомогательный метод для проверки и получения валидного шага
        private Vector2Int ValidateAndGetStep(Vector2Int currentPos, Vector2Int targetPos)
        {
            // Вычисляем разницу между текущей позицией и желаемой целевой позицией
            Vector2Int delta = targetPos - currentPos;

            // Проверяем, что шаг не больше 1 клетки по любой оси
            if (Mathf.Abs(delta.x) > 1 || Mathf.Abs(delta.y) > 1)
            {
                Debug.LogWarning($"A* next step {targetPos} is too far from {currentPos}, calculating step towards");

                // Вычисляем направление к цели с ограничением в 1 шаг
                Vector2Int step = new Vector2Int(
                    Mathf.Clamp(delta.x, -1, 1), // Ограничиваем движение по X одним шагом
                    Mathf.Clamp(delta.y, -1, 1)  // Ограничиваем движение по Y одним шагом
                );

                // Вычисляем следующую позицию как текущая + ограниченный шаг
                Vector2Int nextStep = currentPos + step;

                // Проверяем, что вычисленная позиция проходима
                if (IsWalkable(nextStep))
                {
                    return nextStep; // Возвращаем валидный шаг
                }
                else
                {
                    // Если прямой путь заблокирован, ищем альтернативный обходной шаг
                    return FindAlternativeStep(currentPos, targetPos);
                }
            }

            // Проверяем, что целевая позиция проходима (нет стены или юнита)
            if (!IsWalkable(targetPos))
            {
                Debug.LogWarning($"A* target step {targetPos} is not walkable, finding alternative");
                // Ищем альтернативный путь
                return FindAlternativeStep(currentPos, targetPos);
            }

            // Возвращаем валидный шаг к целевой позиции
            return targetPos;
        }

        // Поиск альтернативного шага, если прямой путь заблокирован
        private Vector2Int FindAlternativeStep(Vector2Int currentPos, Vector2Int targetPos)
        {
            // Определяем все возможные направления движения (включая диагональные)
            Vector2Int[] directions = new Vector2Int[]
            {
                Vector2Int.up,          // Вверх
                Vector2Int.right,       // Вправо
                Vector2Int.down,        // Вниз
                Vector2Int.left,        // Влево
                new Vector2Int(1, 1),   // Диагональ право-вверх
                new Vector2Int(-1, 1),  // Диагональ лево-вверх
                new Vector2Int(1, -1),  // Диагональ право-вниз
                new Vector2Int(-1, -1)  // Диагональ лево-вниз
            };

            // Сортируем направления по близости к целевой точке направление, которое максимально приближает к цели
            var sortedDirections = directions
                .OrderBy(d => Vector2.Distance(
                    new Vector2(currentPos.x + d.x, currentPos.y + d.y),
                    new Vector2(targetPos.x, targetPos.y)
                ))
                .ToArray();

            // Проверяем каждое направление в порядке приоритета
            foreach (Vector2Int direction in sortedDirections)
            {
                Vector2Int alternativeStep = currentPos + direction;

                // Проверяем, что клетка находится в пределах игрового поля
                if (alternativeStep.x >= 0 && alternativeStep.x < runtimeModel.RoMap.Width &&
                    alternativeStep.y >= 0 && alternativeStep.y < runtimeModel.RoMap.Height)
                {
                    // Проверяем, что клетка проходима (нет стены и юнита)
                    if (IsWalkable(alternativeStep))
                    {
                        Debug.Log($"Found alternative step: {alternativeStep} from {currentPos}");
                        return alternativeStep; // Возвращаем найденный альтернативный шаг
                    }
                }
            }

            // Если ни одно направление не подходит, остаемся на текущей позиции
            Debug.LogWarning($"No alternative step found from {currentPos}, staying in place");
            return currentPos;
        }

        // Получение списка соседних клеток для указанной позиции
        private List<Vector2Int> GetNeighbors(Vector2Int position)
        {
            // Создаем пустой список для хранения соседей
            List<Vector2Int> neighbors = new List<Vector2Int>();

            // Определяем 4 основных направления движения (без диагоналей для простоты)
            Vector2Int[] directions = new Vector2Int[]
            {
                Vector2Int.up,    // Вверх (0, 1)
                Vector2Int.down,  // Вниз (0, -1)
                Vector2Int.left,  // Влево (-1, 0)
                Vector2Int.right  // Вправо (1, 0)
            };

            // Проверяем каждое направление
            foreach (Vector2Int direction in directions)
            {
                Vector2Int neighborPos = position + direction; // Вычисляем позицию соседа

                // Проверяем, что сосед находится в пределах игровой карты
                if (neighborPos.x >= 0 && neighborPos.x < runtimeModel.RoMap.Width &&
                    neighborPos.y >= 0 && neighborPos.y < runtimeModel.RoMap.Height)
                {
                    neighbors.Add(neighborPos); // Добавляем валидного соседа в список
                }
            }

            return neighbors; // Возвращаем список соседних клеток
        }

        // Проверка, можно ли пройти через указанную клетку
        private bool IsWalkable(Vector2Int position)
        {
            // Проверяем, что клетка не занята препятствием (стеной)
            if (runtimeModel.RoMap[position])
                return false; // Клетка заблокирована стеной

            // Проверяем, что на клетке нет другого юнита
            // Исключаем начальную и конечную точки пути
            if (position != endPoint && position != startPoint)
            {
                // Проверяем всех юнитов на карте
                if (runtimeModel.RoUnits.Any(u => u.Pos == position))
                    return false; // Клетка занята другим юнитом
            }

            return true; // Клетка проходима
        }

        // Эвристическая функция для оценки расстояния до цели
        private float CalculateHeuristic(Vector2Int from, Vector2Int to)
        {
            
            return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
        }

        // Восстановление полного пути от конечного узла к начальному
        private Vector2Int[] ReconstructPath(PathNode endNode)
        {
            // Создаем список для хранения точек пути
            List<Vector2Int> pathList = new List<Vector2Int>();
            PathNode currentNode = endNode; // Начинаем с конечного узла

            // Движемся от конечного узла к начальному по родительским ссылкам
            while (currentNode != null)
            {
                pathList.Add(currentNode.Position); // Добавляем позицию текущего узла
                currentNode = currentNode.Parent; // Переходим к родительскому узлу
            }

            // Переворачиваем список, чтобы путь шел от начала к концу
            pathList.Reverse();

            // Преобразуем список в массив и возвращаем
            return pathList.ToArray();
        }
    }
}