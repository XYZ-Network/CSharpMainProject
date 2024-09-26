using Model;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Pathfinding;
using UnityEngine;

namespace Assets.Scripts.UnitBrains.Pathfinding
{
    public class SmartUnitPath : BaseUnitPath
    {
        private Vector2Int _startPosition;
        private Vector2Int _targetPosition;
        private int[] dx = { -1, 0, 1, 0 };
        private int[] dy = { 0, 1, 0, -1 };

        private bool _isTarget;
        private bool _isEnemyUnitClose;
        private SmartNode _nextToEnemyUnit;

        public SmartUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPosition, Vector2Int targetPosition) :
            base(runtimeModel, startPosition, targetPosition)
        {
            _startPosition = startPosition;
            _targetPosition = targetPosition;
        }

        protected override void Calculate()
        {
            
            SmartNode startNode = new SmartNode(_startPosition);// Задаёт стартовые координаты
            SmartNode targetNode = new SmartNode(_targetPosition);// Задаёт координаты цели
            List<SmartNode> openList = new List<SmartNode> { startNode };// В список вносятся вершины в которые можно пойти
            List<SmartNode> closedList = new List<SmartNode>();// В список вносятся пройденные вершины, которые не участвуют в вычислениях

            int counter = 0;
            int maxCount = runtimeModel.RoMap.Width * runtimeModel.RoMap.Height;// Ограничивает максимум шагов размерами карты

            while (openList.Count > 0 && counter++ < maxCount)// Цикл выполняется пока в openList ещё есть ноды
            {
                SmartNode currentNode = openList[0];// Выбирается первая нода из списка (индексация начинается с 0)

                foreach (var node in openList)
                {
                    if (node.Value < currentNode.Value)// Перебирает ноды в списке и ищет с наименьшим значением эвристической функции
                        currentNode = node;// Делает такую ноду текущей
                }

                openList.Remove(currentNode);//Раз эта нода пройдена, то она исключается из открытого списка
                closedList.Add(currentNode);// И зачисляется в закрытый

                if (_isTarget)
                {
                    path = FindPath(currentNode);
                    return;
                }

                for (int i = 0; i < dx.Length; i++)
                {
                    // Складывают координату текущей ноды и её смещение по оси, и выдает новую координату по Х и Y соответственно
                    int newX = currentNode.Position.x + dx[i];
                    int newY = currentNode.Position.y + dy[i];
                    var newPosition = new Vector2Int(newX, newY);//Новая позиция 

                    if (newPosition == targetNode.Position)// Проверяет, будет ли следующая позиция целевой
                        _isTarget = true;

                    if (IsValid(newPosition) || _isTarget)// Если клетка доступна для хода и она является целевой
                    {
                        SmartNode neighbor = new SmartNode(newPosition);// Для неё создаётся нода

                        if (closedList.Contains(neighbor))// Проверяем, что этой ноды нет в закрытом списке
                            continue;

                        neighbor.Parent = currentNode;// Указываем в направлении текущую ноду
                        neighbor.CalculateEstimate(targetNode.Position);// Рассчитываем расстояние
                        neighbor.CalculateValue();// И стоимость эвристической функции

                        openList.Add(neighbor);// Добавляем ноду в открытый список
                    }
                    if (CheckCollisionWithEnemy(newPosition) && !_isEnemyUnitClose)
                    {
                        _isEnemyUnitClose = true;
                        _nextToEnemyUnit = currentNode;
                    }
                }
            }
            if (_isEnemyUnitClose)
            {
                path = FindPath(_nextToEnemyUnit);
                return;
            }

            path = new Vector2Int[] { startNode.Position };
        }

        private Vector2Int[] FindPath(SmartNode currentNode)
        {
            List<Vector2Int> path = new();

            while (currentNode != null)// Цикл движется в обратном порядке, пока currentNode имеет значение
            {
                path.Add(currentNode.Position);// Помещает текущую ноду в список "путь"
                currentNode = currentNode.Parent;// Подставляет под текущую ноду следующую из Parent
            }

            path.Reverse();// Разворачивает список в обратном (правильном) порядке
            return path.ToArray();// Возвращает список пути начиная с координаты врага
        }

        private bool IsValid(Vector2Int point)//Проверяет проходимость клетки на карте
        {
            return runtimeModel.IsTileWalkable(point);
        }

        private bool CheckCollisionWithEnemy(Vector2Int newPos)//Проверяет столкновение с врагом
        {
            var botUnitPositions = runtimeModel.RoBotUnits.Select(u => u.Pos).Where(u => u == newPos);

            return botUnitPositions.Any();
        }
    }
}