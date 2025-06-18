using System.Collections.Generic;
using System.Linq;
using Model;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    public class AStarUnitPath : BaseUnitPath
    {
        public AStarUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint)
            : base(runtimeModel, startPoint, endPoint)
        { }

        protected override void Calculate()
        {
            if (startPoint == endPoint)
            {
                path = new[] { startPoint };
                return;
            }

            var openSet = new PriorityQueue<Node>();
            var closedSet = new HashSet<Vector2Int>();
            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            var gScore = new Dictionary<Vector2Int, int> { [startPoint] = 0 };
            var fScore = new Dictionary<Vector2Int, int> { [startPoint] = Heuristic(startPoint, endPoint) };

            openSet.Enqueue(new Node(startPoint, fScore[startPoint]));

            while (openSet.Count > 0)
            {
                var current = openSet.Dequeue();

                if (current.Position == endPoint)
                {
                    path = ReconstructPath(cameFrom, current.Position);
                    return;
                }

                closedSet.Add(current.Position);

                foreach (var neighbor in GetWalkableNeighbors(current.Position, closedSet))
                {
                    int tentativeGScore = gScore[current.Position] + 1;

                    if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current.Position;
                        gScore[neighbor] = tentativeGScore;
                        fScore[neighbor] = tentativeGScore + Heuristic(neighbor, endPoint);

                        if (!openSet.Contains(neighbor))
                            openSet.Enqueue(new Node(neighbor, fScore[neighbor]));
                    }
                }
            }

            path = new[] { startPoint }; // fallback
        }

        private IEnumerable<Vector2Int> GetWalkableNeighbors(Vector2Int pos, HashSet<Vector2Int> closedSet)
        {
            foreach (var dir in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                var neighbor = pos + dir;
                if (neighbor.x < 0 || neighbor.x >= runtimeModel.RoMap.Width ||
                    neighbor.y < 0 || neighbor.y >= runtimeModel.RoMap.Height)
                    continue;
                if (!runtimeModel.IsTileWalkable(neighbor))
                    continue;
                if (closedSet.Contains(neighbor))
                    continue;
                yield return neighbor;
            }
        }

        private Vector2Int[] ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
        {
            var totalPath = new List<Vector2Int> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                totalPath.Insert(0, current);
            }
            return totalPath.ToArray();
        }

        private int Heuristic(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y); // Manhattan distance
        }

        private class Node
        {
            public Vector2Int Position { get; }
            public int FScore { get; }

            public Node(Vector2Int pos, int fScore)
            {
                Position = pos;
                FScore = fScore;
            }
        }

        private class PriorityQueue<T> where T : Node
        {
            private readonly List<T> _elements = new();

            public int Count => _elements.Count;

            public bool Contains(Vector2Int pos) => _elements.Any(n => n.Position == pos);

            public void Enqueue(T item)
            {
                _elements.Add(item);
                _elements.Sort((a, b) => a.FScore.CompareTo(b.FScore));
            }

            public T Dequeue()
            {
                if (_elements.Count == 0)
                    return null;
                var item = _elements[0];
                _elements.RemoveAt(0);
                return item;
            }
        }
    }
}
