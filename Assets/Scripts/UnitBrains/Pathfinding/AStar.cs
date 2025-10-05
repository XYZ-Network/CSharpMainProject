using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitBrains.Pathfinding;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using static UnityEngine.GraphicsBuffer;

namespace Assets.Scripts.UnitBrains.Pathfinding
{
   
    public class AStar : BaseUnitPath
    {
        private class Node
        {
            public int X;
            public int Y;
            public int G; // Стоимость пути от старта
            public int H; // Эвристическая оценка до цели
            public int Value => G + H;
            public Node Parent;

            public Node(int x, int y)
            {
                X = x;
                Y = y;
                G = int.MaxValue;
            }
            //Старт: (1, 1), Цель: (4, 5) → H = |1-4| + |1-5| = 3 + 4 = 7.
            //Алгоритм A* использует F = G + H для выбора оптимального пути:
            //G — точная стоимость пройденного пути.
            //H — оценка оставшегося пути (чем меньше, тем приоритетнее узел).


            public void CalculateEstimate(int targetX, int targetY)
            {
                H = Math.Abs(X - targetX) + Math.Abs(Y - targetY);
            }

            public override bool Equals(object obj) => obj is Node node && X == node.X && Y == node.Y;
            public override int GetHashCode() => HashCode.Combine(X, Y);
            public Vector2Int ToVector() => new(X, Y);
        }

        private readonly int[] dx = { -1, 0, 1, 0 };
        private readonly int[] dy = { 0, 1, 0, -1 };

        private const int WaitFrames = 60;//колво фреймов ожидания
        private int waitCounter = 0;//счетчик ожидания

        private Vector2Int lastBlockedPosition;
        private Vector2Int _currentPosition;

        public AStar(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint)
            : base(runtimeModel, startPoint, endPoint)
        {
            _currentPosition = startPoint;
        }

        public override Vector2Int GetNextStepFrom(Vector2Int currentPos)
        {
            //Если юнит уже переместился в новую клетку (не совпадает с _currentPosition), обновляем его позицию и сбрасываем сохранённый путь
            if (currentPos != _currentPosition)
            {
                _currentPosition = currentPos;
                path = null;
            }
            // Вызываем алгоритм A* для поиска пути
            if (path == null || path.Length == 0)
            {
                Calculate();
            }
            //Если путь после расчёта остался пустым, возвращаем текущую позицию.Стоим на месте
            if (path == null || path.Length == 0)
            {
                return _currentPosition;
            }

            int currentIndex = Array.IndexOf(path, _currentPosition);//проверяем есть ли текущая клетка в массиве типа VectorInt2 path
            if (currentIndex < 0)// путь не актуален перерассчитываем
            {
                path = null;
                Calculate();
                currentIndex = Array.IndexOf(path, _currentPosition);
                if (currentIndex < 0) return _currentPosition;// Если снова не найден — стоим
            }

            return currentIndex + 1 < path.Length ? path[currentIndex + 1] : _currentPosition;// Если следующая точка в пути существует — возвращаем её,
                                                                                              // если юнит уже достиг конца пути — возвращаем текущую позицию 

        }

        protected override void Calculate()
        {
            if (waitCounter > 0)
            {
                waitCounter--;
                if (CheckNeighborsAvailable())
                {
                    waitCounter = 0;
                    CalculatePath();
                }
                return;
            }

            CalculatePath();
        }

        private void CalculatePath()
        {
            path = null;

            if (_currentPosition == endPoint)//сравниваем екущие координаты юнита  с целевой точкой 
            {
                path = new[] { _currentPosition };// создает массив пути из одного элемента 
                return;//перрываем дальнейшие вычисления
            }

            Node startNode = new Node(_currentPosition.x, _currentPosition.y) { G = 0 };
            Node targetNode = new Node(endPoint.x, endPoint.y);
            startNode.CalculateEstimate(targetNode.X, targetNode.Y);

            var openList = new List<Node> { startNode };
            var closedList = new HashSet<Node>();
            var allNodes = new Dictionary<Vector2Int, Node> { [_currentPosition] = startNode };

            Node bestNode = startNode;
            bool pathBlocked = true;

            while (openList.Count > 0)
            {
                Node currentNode = GetBestNode(openList);

                if (currentNode.X == targetNode.X && currentNode.Y == targetNode.Y)//при совпадении текущей точки с конечной строим путь
                {
                    ReconstructPath(currentNode);
                    return;
                }

                if (currentNode.H < bestNode.H)// если эвристика текущей меньше чем у лучшей назначаем ее лучшей
                {
                    bestNode = currentNode;
                }

                openList.Remove(currentNode);// убираем и открытого спсика эту ноду
                closedList.Add(currentNode);// добавляем в закрытый список, уже пройденных нод

                //обработка соседнгих клеток
                for (int i = 0; i < dx.Length; i++)
                {
                    Vector2Int neighborPos = new(currentNode.X + dx[i], currentNode.Y + dy[i]);//создаем переменную векторного типа обохначающую соседнюю клетку

                    if (!IsValid(neighborPos))// если проходит по условиям "валидности" переходим к след шагу
                        continue;

                    if (!allNodes.TryGetValue(neighborPos, out Node neighbor))//проверяем есть ли этот сосед в обработанных точках ранее
                    {
                        neighbor = new Node(neighborPos.x, neighborPos.y);
                        neighbor.CalculateEstimate(targetNode.X, targetNode.Y);
                        allNodes[neighborPos] = neighbor;// в случае если не был, то добавляем в словарь для возможного дальнецшего доступа
                    }

                    if (closedList.Contains(neighbor))// проверяем есть ли в писке уже пройденных,нет? переходим к след шагу
                        continue;

                    int tentativeG = currentNode.G + GetMovementCost(neighborPos);//расчиытваем стоимость пути до соседнего узла

                    if (tentativeG < neighbor.G || !openList.Contains(neighbor))// если новая стоимость пути лучше прежней и данный сосед не добавлен в открытый список
                    {                                                                //назначаем ей новую стоимость и добавляем в родительскую ноду
                        neighbor.G = tentativeG;                                         //отмечаем что она не заблокирована
                        neighbor.Parent = currentNode;
                        pathBlocked = false;                                                           
                  
                        if (!openList.Contains(neighbor))
                            openList.Add(neighbor);
                    }
                }
            }

            if (pathBlocked)//если путь заблокирован
            {
                lastBlockedPosition = _currentPosition;// в переменную последняя заблоченная позиция передаем координаты нашей точки
                waitCounter = WaitFrames; // передаем значение "таймера" в  счетчик ожидания
            }
            else
            {
                ReconstructPath(bestNode);//если нет строим путь
            }
        }

        private int GetMovementCost(Vector2Int pos)
        {
            // Можно добавить разные стоимости для разных типов метсности
            return runtimeModel.RoUnits.Any(u => u.Pos == pos) ? 30 : 10;
        }

        private bool CheckNeighborsAvailable()//функция реализует проверку соседних клеток на доступность 
        {
            for (int i = 0; i < dx.Length; i++)
            {
                Vector2Int checkPos = new(
                    lastBlockedPosition.x + dx[i],
                    lastBlockedPosition.y + dy[i]);

                if (IsValid(checkPos))
                    return true;
            }
            return false;
        }

        private Node GetBestNode(List<Node> nodes)//функция реализует сравнение нод из открытого списка и выдает самую лучшую по стоимости и длине пути
        {
            Node best = nodes[0];
            for (int i = 1; i < nodes.Count; i++)
            {
                if (nodes[i].Value < best.Value ||
                   (nodes[i].Value == best.Value && nodes[i].H < best.H))
                {
                    best = nodes[i];
                }
            }
            return best;
        }

        private void ReconstructPath(Node endNode)// восстановление пути
        {
            List<Vector2Int> pathList = new();
            Node current = endNode;

            while (current != null)
            {
                pathList.Add(current.ToVector());
                current = current.Parent;
            }

            pathList.Reverse();
            path = pathList.ToArray();
        }

        private bool IsValid(Vector2Int pos)// проверка валидности точки
        {
            if (pos.x < 0 || pos.x >= runtimeModel.RoMap.Width ||
                pos.y < 0 || pos.y >= runtimeModel.RoMap.Height)
                return false;

            if (runtimeModel.RoMap[pos])
                return false;

            if (pos == _currentPosition || pos == endPoint)
                return true;

            return !runtimeModel.RoUnits.Any(u => u.Pos == pos);
        }
    }
}
