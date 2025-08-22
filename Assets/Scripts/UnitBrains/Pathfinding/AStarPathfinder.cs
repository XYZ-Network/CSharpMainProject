using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Pathfinding;
using UnityEngine;

namespace Assets.Scripts.UnitBrains.Pathfinding
{
    public class AStarPathfinder : BaseUnitPath
    {
        private readonly int[] dx = { -1, 0, 1, 0 };
        private readonly int[] dy = { 0, 1, 0, -1 };

        // пул для повторного использования PathNode объектов
        private readonly ObjectPool<GridNode> nodePool = new();

        public AStarPathfinder(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint)
            : base(runtimeModel, startPoint, endPoint) { }

        protected override void Calculate()
        {
            var openList = new List<GridNode>();

            // словарь родителя каждой точки
            var cameFrom = new Dictionary<Vector2Int, GridNode>();
            // стоимость от начальной точки до каждой точки
            var gScore = new Dictionary<Vector2Int, int>();
            // занятые точки
            var visited = new HashSet<Vector2Int>();

            Vector2Int start = startPoint;
            Vector2Int target = endPoint;

            GridNode startNode = nodePool.Get(start.x, start.y);
            // стоимость начальной точки
            startNode.Cost = 0;
            // рассчет эвристики
            startNode.CalculateEstimate(target.x, target.y);
            // итоговая стоимость
            startNode.CalculateValue();

            openList.Add(startNode);
            gScore[start] = 0;

            while (openList.Count > 0)
            {
                // узел с наименьшей стоимостью Value
                GridNode current = openList.OrderBy(n => n.Value).First();
                openList.Remove(current);

                Vector2Int currentPos = new(current.X, current.Y);

                if (Math.Abs(current.X - target.x) <= 1 && Math.Abs(current.Y - target.y) <= 1)
                {
                    var pathList = new List<Vector2Int>();
                    while (current != null)
                    {
                        pathList.Add(new Vector2Int(current.X, current.Y));
                        current = current.Parent;
                    }
                    pathList.Reverse();
                    path = pathList.ToArray();
                    nodePool.Clear();
                    return;
                }

                visited.Add(currentPos);

                for (int i = 0; i < dx.Length; i++)
                {
                    int newX = current.X + dx[i];
                    int newY = current.Y + dy[i];
                    Vector2Int neighborPos = new(newX, newY);

                    // пропускаем недопустимые или уже посещённые клетки
                    if (!IsValid(neighborPos) || visited.Contains(neighborPos))
                        continue;

                    // стоимость пути через текущий узел
                    int tentativeG = gScore[currentPos] + 1;

                    // если этот путь лучше предыдущего к ближайшему
                    if (!gScore.ContainsKey(neighborPos) || tentativeG < gScore[neighborPos])
                    {
                        GridNode neighbor = nodePool.Get(newX, newY);
                        neighbor.Parent = current;

                        // проверка блокируют ли юниты клетку
                        neighbor.Cost = runtimeModel.RoUnits.All(u => u.Pos != neighborPos) ? 0 : 3;
                        neighbor.CalculateEstimate(target.x, target.y);
                        neighbor.CalculateValue();

                        gScore[neighborPos] = tentativeG;
                        cameFrom[neighborPos] = current;

                        if (!openList.Contains(neighbor))
                            openList.Add(neighbor);
                    }
                }
            }

            // если путь не найден
            path = new[] { StartPoint };
            nodePool.Clear();
        }

        /// <summary>
        /// Находится ли точка в границах и проходима ли она
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private bool IsValid(Vector2Int point)
        {
            return point.x >= 0 && point.y >= 0 &&
                   point.x < runtimeModel.RoMap.Width &&
                   point.y < runtimeModel.RoMap.Height &&
                   !runtimeModel.RoMap[point];
        }
    }
}