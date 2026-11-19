using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    internal class Node
    {
        public Vector2Int Position;
        public int Cost; //цена пути от старта до клетки
        public int Estimate; //оценка расстояния от клетки до цели
        public Node Parent; // ссылка на предыдушую клетку
        public int Value => Cost + Estimate;// какую клетку рассматриваем следующей

        public Node (Vector2Int position, int cost, int estimate, Node parent = null)
        {
            Position = position;
            Cost = cost;
            Estimate = estimate;
            Parent = parent;
        }
    }
    
    public class AStarUnitPath : BaseUnitPath
    {
        private static readonly int[] dx = { -1, 0, 1, 0}; // направление по x
        private static readonly int[] dy = { 0, 1, 0, -1}; // напрвление по y
        public AStarUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint) : base(runtimeModel, startPoint, endPoint)
        {
        }
        protected override void Calculate()
        {
            if (TryFindPath(false))
            {
                return;
            }
            if (TryFindPath(true))
            {
                return;
            }
            path = new[] { startPoint };
        }
        private bool TryFindPath(bool ignoreUnits)
        {
            Node startNode = new Node(startPoint, 0, CalculateEstimate(startPoint, endPoint)); // создаём стартовую ноду и передаём ей параментры текущую позицию юнита, стоимость пути, расстояние от клетки до цели
            List<Node> openList = new List<Node>() { startNode}; // создаём список нод которые будем проверять на поиск удачного пути. После проверки нода будет попадать в закрытый список
            HashSet<Vector2Int> closedList = new HashSet<Vector2Int>(); // создаём список проверенных клеток
            while (openList.Count > 0) // пока список не пуст
            {
                Node currentNode = openList[0]; // запоминаем текущую клетку, чтобы найти рядом с ней клетку с наименьшим Value
                foreach (var node in openList) //проходимся по всем нодам в открытом списке
                {
                    if (node.Value < currentNode.Value) // проверяем что value проверяемой клетки меньше текущей клетки
                    {
                        currentNode = node; // запоминаем клетку с наименьшим значем value
                    }
                }
                if (currentNode.Position == endPoint) // проверяем не дошли ли мы до цели
                {
                    path = BuildPath(currentNode); // строим путь до цели
                    return true; // путь найден
                }

                openList.Remove(currentNode);// удаляем провернную клетку из открытого списка
                closedList.Add(currentNode.Position); // записываем позицию проверенной клетки в закрытый список

                List<Vector2Int> neighbours = GetWalkableNeighbours(currentNode.Position, ignoreUnits); // записываем список доступных клеток для перехода
                foreach (var neighbour in neighbours) // проходимся по каждому элементу в списке
                {
                    if (closedList.Contains(neighbour)|| openList.Exists(n => n.Position == neighbour))// проверяем нет ли клетки в списке проверенных или в проверяемых. Если есть то пропускаем
                    {
                        continue;
                    }
                    Node newNode = new Node(neighbour, currentNode.Cost + 1, CalculateEstimate(neighbour, endPoint), currentNode);
                    openList.Add(newNode);
                }

            }
            return false; // путь не найден
        }


        private List<Vector2Int> GetWalkableNeighbours(Vector2Int position, bool ignoreUnits)
        {
            List<Vector2Int> positions = new List<Vector2Int>();
            for (int i = 0; i < dx.Length; i++)
            {
                var neighbour = new Vector2Int(position.x + dx[i], position.y + dy[i]); // записываем в цикле все соседние клетки
                if (neighbour == endPoint) // база всегда доступна
                {
                    positions.Add(neighbour); // добавляем в список базу
                    continue;
                }
                bool walkable = ignoreUnits ? !runtimeModel.RoMap[neighbour] : runtimeModel.IsTileWalkable(neighbour);

                if (walkable)
                {
                    positions.Add(neighbour);
                }
            }
            return positions; // возвращаем список доступных клеток
        }
        private int CalculateEstimate(Vector2Int from, Vector2Int to)
        {
            return Math.Abs(from.x - to.x) + Math.Abs(from.y - to.y); //считаем расстояние до цели
        }

        private Vector2Int[] BuildPath (Node endNode)
        {
            List<Vector2Int> pathToEnd = new List<Vector2Int>();
            while (endNode != null) // проходимся циклов от конечной клетки до стартовой
            {
                pathToEnd.Add(endNode.Position); // записываем позицию в списрк вектора
                endNode = endNode.Parent; // шаг назад по цепочке. Выбираем из какой клетки пришли в текущую
            }
            pathToEnd.Reverse(); // переворачиваем список, чтобы путь был от стартовой клетки к последней клетке
            return pathToEnd.ToArray(); // возвращаем массив пути
        }
    }
}
