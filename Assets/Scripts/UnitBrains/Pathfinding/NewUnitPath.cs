using System;
using System.Collections.Generic;
using Model;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    public class NewUnitPath : BaseUnitPath
    {
        private int[] dx = { -1, 0, 1, 0 };
        private int[] dy = { 0, 1, 0, -1 };

        public NewUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint) : base(
            runtimeModel, startPoint, endPoint)
        {
        }

        protected override void Calculate()
        {
            path = FindPath().ToArray(); //записываем результат метода в путь, но массивом

            if (path == null)
                path = new Vector2Int[] { StartPoint }; //если путь вернул null, то записываем как путь просто стартовую точку юнита
        }

        public List<Vector2Int> FindPath()
        {
            Node startNode = new Node(startPoint); //задаем как стартовую ноду позицию нашего юнита
            Node targetNode = new Node(endPoint); //задаем как ноду цели - позицию юнита врага

            List<Node> openList = new List<Node> { startNode }; // сюда добавляются все вершины, в которые можно пойти
            List<Node> closedList = new List<Node>(); // сюда добавляются все вершины, по которым уже прошли, они в вычислениях не участвуют

            while (openList.Count > 0) //цикл будет работать, пока список openList не станет пустым
            {
                Node currentNode = openList[0]; //устанавливаем текущую ноду как стартовую точку = первую ноду в списке OpenList

                foreach (var node in openList) //проходимся по всему списку, чтобы найти ноды с наименьшей стоимостью 
                {
                    if (node.Value < currentNode.Value) //условие поиска наименьшей стоимости среди всех в OpenList
                        currentNode = node; //если  находим, то присваиваем ее как текущую
                }

                openList.Remove(currentNode); //как только нашли ноду - удаляем ее из OpenList
                closedList.Add(currentNode); //добавляем ее в ClosedList, тем самым обозначив, что ее больше не смотрим

                if (currentNode.Position.x == targetNode.Position.x && currentNode.Position.y == targetNode.Position.y) //проверяем, добрались ли мы до цели
                {
                    List<Vector2Int> path = new List<Vector2Int>(); //если да, то начинаем формировать путь из нод
                    while (currentNode != null)
                    {
                        path.Add(currentNode.Position); //формируем список, добавляя координаты каждого узла, пока он не станет null
                        currentNode = currentNode.Parent; //записываем родителя для того, чтобы потом двигаться по пути в обратную сторону
                    }

                    path.Reverse(); //делаем реверс пути, чтобы рассчитать путь от врага до нас
                    return path; //возвращаем путь
                } //если нет, то продолжаем поиск

                for (int i = 0; i < dx.Length; i++) //проходимся циклом по соседним клеткам
                {
                    Vector2Int newPos = new Vector2Int(currentNode.Position.x + dx[i], currentNode.Position.y + dy[i]); //соседние клетки записываем в newpos

                    if (!runtimeModel.IsTileWalkable(newPos) && endPoint != newPos) //проверяем доступность соседней клетки или что соседняя клетка не равна цели
                        continue; //если клетка не доступна или является целью - то прерываем текущую итерацию цикла

                    Node neighbor = new Node(newPos); //если доступна, то формируем ее в соседа

                    if (closedList.Contains(neighbor)) //проверяем, нет ли такой уже в списке закрытых
                        continue; //если есть, то прерываем текущую итерацию цикла

                    neighbor.Parent = currentNode; //если нет, то записываем текущую в парент 
                    neighbor.CalculateEstimate(targetNode.Position.x, targetNode.Position.y); //рассчитываем путь 
                    neighbor.CalculateValue(); //рассчитываем значение

                    openList.Add(neighbor); // после всех проверок и расчетов, добавляем соседа в openlist 
                }
            }

            return null; //если путь не найден, то возвращаем налл
        }
    }

    public class Node
    {
        public Vector2Int Position;
        public int Cost;
        public int Estimate;
        public int Value;
        public Node Parent;

        public Node(Vector2Int position)
        {
            Position = position;
        }

        public void CalculateEstimate(int targetX, int targetY)
        {
            Estimate = Mathf.Abs(Position.x - targetX) + Mathf.Abs(Position.y - targetY);
        }

        public void CalculateValue()
        {
            Value = Cost + Estimate;
        }

        public override bool Equals(object obj)
        {
            if (obj is not Node node)
                return false;

            return Position.Equals(node.Position);
        }
    }
}