using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    public class AStarUnitPath : BaseUnitPath
    {
        private static readonly Vector2Int[] Directions =
        {
            new Vector2Int( 1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int( 0, 1),
            new Vector2Int( 0,-1),
        };

        public AStarUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint)
            : base(runtimeModel, startPoint, endPoint)
        {
        }

        protected override void Calculate()
        {
            var goal = ResolveGoal(endPoint);

            if (startPoint == goal)
            {
                path = new[] { startPoint };
                return;
            }

            if (startPoint == goal)
            {
                path = new[] { startPoint };
                return;
            }

            var open = new List<Vector2Int> { startPoint };
            var openSet = new HashSet<Vector2Int> { startPoint };
            var closed = new HashSet<Vector2Int>();

            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();

            var gScore = new Dictionary<Vector2Int, int> { [startPoint] = 0 };
            var fScore = new Dictionary<Vector2Int, int> { [startPoint] = Heuristic(startPoint, goal) };

            const int MAX_ITERS = 20000;
            var iters = 0;
            var bestSoFar = startPoint;
            var bestSoFarH = Heuristic(startPoint, goal);

            while (open.Count > 0)
            {
                if (++iters > MAX_ITERS)
                {
                    Debug.LogWarning($"AStar exceeded MAX_ITERS={MAX_ITERS}. Returning fallback path.");
                    path = new[] { startPoint };
                    return;
                }

                var current = SelectBest(open, fScore, goal);
                var currentH = Heuristic(current, goal);
                if (currentH < bestSoFarH)
                {
                    bestSoFarH = currentH;
                    bestSoFar = current;
                }

                if (current == goal)
                {
                    path = ReconstructPath(cameFrom, current).ToArray();
                    return;
                }

                open.Remove(current);
                openSet.Remove(current);
                closed.Add(current);

                foreach (var dir in Directions)
                {
                    var neighbor = current + dir;

                    if (closed.Contains(neighbor))
                        continue;

                    if (!IsWalkableOrGoal(neighbor, goal))
                        continue;

                    var tentativeG = GetScore(gScore, current) + 1;

                    if (!openSet.Contains(neighbor))
                    {
                        open.Add(neighbor);
                        openSet.Add(neighbor);
                    }
                    else
                    {
                        if (tentativeG >= GetScore(gScore, neighbor))
                            continue;
                    }

                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, goal);
                }
            }

            if (bestSoFar != startPoint)
                path = ReconstructPath(cameFrom, bestSoFar).ToArray();
            else
                path = new[] { startPoint };
        }

        private bool IsOccupiedByUnit(Vector2Int cell)
        {
            foreach (var u in runtimeModel.RoUnits)
            {
                if (u.Pos == cell)
                    return true;
            }
            return false;
        }

        private Vector2Int ResolveGoal(Vector2Int desired)
        {
            if (runtimeModel.IsTileWalkable(desired))
                return desired;

            var visited = new HashSet<Vector2Int>();
            var q = new Queue<Vector2Int>();

            // стартуем не с desired (он непроходим), а с его соседей
            foreach (var dir in Directions)
            {
                var n = desired + dir;
                if (visited.Add(n))
                    q.Enqueue(n);
            }

            const int MAX_NODES = 5000; // защита
            var processed = 0;

            while (q.Count > 0 && processed++ < MAX_NODES)
            {
                var cur = q.Dequeue();
                if (runtimeModel.IsTileWalkable(cur))
                    return cur;

                foreach (var dir in Directions)
                {
                    var n = cur + dir;
                    if (visited.Add(n))
                        q.Enqueue(n);
                }
            }

            // если совсем нет проходимых — возвращаем старт (пути нет)
            return startPoint;
        }

        private bool IsWalkableOrGoal(Vector2Int cell, Vector2Int goal)
        {
            if (cell == startPoint) return true;
            if (cell == goal) return true;

            if (!runtimeModel.IsTileWalkable(cell))
                return false;

            if (IsOccupiedByUnit(cell))
                return false;

            return true;
        }

        private static int Heuristic(Vector2Int a, Vector2Int b)
            => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y); // Manhattan

        private static int GetScore(Dictionary<Vector2Int, int> scores, Vector2Int key)
            => scores.TryGetValue(key, out var v) ? v : int.MaxValue / 4;

        private static Vector2Int SelectBest(List<Vector2Int> open, Dictionary<Vector2Int, int> fScore, Vector2Int goal)
        {
            var best = open[0];
            var bestF = GetScore(fScore, best);
            var bestH = Heuristic(best, goal);

            for (int i = 1; i < open.Count; i++)
            {
                var v = open[i];
                var f = GetScore(fScore, v);
                if (f > bestF) continue;

                var h = Heuristic(v, goal);
                if (f < bestF || h < bestH)
                {
                    best = v;
                    bestF = f;
                    bestH = h;
                }
            }

            return best;
        }

        private static IEnumerable<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
        {
            var stack = new Stack<Vector2Int>();
            stack.Push(current);

            while (cameFrom.TryGetValue(current, out var prev))
            {
                current = prev;
                stack.Push(current);
            }

            while (stack.Count > 0)
                yield return stack.Pop();
        }
    }
}
