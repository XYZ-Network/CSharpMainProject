using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    public class Node
    {
        public Vector2Int Pos;
        public int G = int.MaxValue; // cost from start
        public int H; // heuristic
        public int F; // G + H
        public Node Parent;

        public Node(Vector2Int pos)
        {
            Pos = pos;
        }

        public override bool Equals(object obj)
        {
            if (obj is Node node)
                return Pos.x == node.Pos.x && Pos.y == node.Pos.y;

            return false;
        }

        public override int GetHashCode() => (Pos.x * 397) ^ Pos.y;
    }

    public class AStarUnitPath : BaseUnitPath
    {
        public AStarUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint)
            : base(runtimeModel, startPoint, endPoint)
        {
        }

        protected override void Calculate()
        {
            if (startPoint == endPoint)
            {
                path = new[] { startPoint };
                return;
            }

            Node startNode = new Node(startPoint);
            Node targetNode = new Node(endPoint);

            startNode.G = 0;
            startNode.H = ManhattanDistance(startNode, targetNode);
            startNode.F = startNode.G + startNode.H;

            int maxDistance = ManhattanDistance(startNode, targetNode);
            int tryDistance = 0;

            // если путь полностью заблокирован, пытаемся найти частичный путь ближе к цели
            while ((path == null || path.Length == 0) && tryDistance <= maxDistance)
            {
                path = CalculatePath(startNode, targetNode, tryDistance)
                    .Select(node => node.Pos)
                    .ToArray();
                tryDistance++;
            }
        }

        public List<Node> CalculatePath(Node start, Node target, int distance)
        {
            var openList = new List<Node> { start };
            var closedSet = new HashSet<Node>();

            while (openList.Count > 0)
            {
                var current = openList
                    .OrderBy(n => n.F)
                    .First();

                if (ManhattanDistance(current, target) <= distance)
                    return ReconstructPath(current);

                openList.Remove(current);
                closedSet.Add(current);

                foreach (var neighbor in GetNeighbors(current))
                {
                    if (closedSet.Contains(neighbor) || !IsValid(neighbor))
                        continue;

                    int tentativeG = current.G + MovementCost(current, neighbor);

                    var inOpen = openList.FirstOrDefault(n => n.Equals(neighbor));
                    if (inOpen == null)
                    {
                        neighbor.G = tentativeG;
                        neighbor.H = ManhattanDistance(neighbor, target);
                        neighbor.F = neighbor.G + neighbor.H;
                        neighbor.Parent = current;
                        openList.Add(neighbor);
                    }
                    else
                    {
                        // если найден более короткий путь к соседу — обновляем
                        if (tentativeG < inOpen.G)
                        {
                            inOpen.Parent = current;
                            inOpen.G = tentativeG;
                            inOpen.F = inOpen.G + inOpen.H;
                        }
                    }
                }
            }

            return new List<Node>();
        }

        private int MovementCost(Node from, Node to) => 1;

        public int ManhattanDistance(Node a, Node b) =>
            Math.Abs(a.Pos.x - b.Pos.x) + Math.Abs(a.Pos.y - b.Pos.y);

        public List<Node> ReconstructPath(Node node)
        {
            var path = new List<Node>();
            while (node != null)
            {
                path.Add(node);
                node = node.Parent;
            }
            path.Reverse();
            return path;
        }

        public List<Node> GetNeighbors(Node node)
        {
            var neighbors = new List<Node>();
            Vector2Int[] directions =
            {
                Vector2Int.up,
                Vector2Int.right,
                Vector2Int.down,
                Vector2Int.left
            };

            foreach (var dir in directions)
                neighbors.Add(new Node(node.Pos + dir));

            return neighbors;
        }

        public bool IsValid(Node node)
        {
            bool validX = node.Pos.x >= 0 && node.Pos.x < runtimeModel.RoMap.Width;
            bool validY = node.Pos.y >= 0 && node.Pos.y < runtimeModel.RoMap.Height;
            bool walkable = runtimeModel.IsTileWalkable(node.Pos);
            return validX && validY && walkable;
        }
    }
}
