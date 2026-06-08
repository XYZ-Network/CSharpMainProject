using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    public class SmartPath : BaseUnitPath
    {
        public SmartPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint)
            : base(runtimeModel, startPoint, endPoint)
        {
        }

        protected override void Calculate()
        {
            var pathNodes = FindPath(startPoint, endPoint);

            if (pathNodes == null || pathNodes.Count == 0)
            {
                path = new[] { startPoint }; 
                return;
            }

            path = pathNodes
                .Select(n => n.Pos)
                .ToArray();
        }

        private List<Node> FindPath(Vector2Int startPos, Vector2Int targetPos)
        {
            var start = new Node(startPos);
            var target = new Node(targetPos);

            var open = new List<Node> { start };
            var closed = new List<Node>();

            while (open.Count > 0)
            {
                Node current = open.OrderBy(n => n.Value).First();

                if (current.Pos == target.Pos)
                    return BuildPath(current);

                open.Remove(current);
                closed.Add(current);

                foreach (var neighbor in GetNeighbors(current))
                {
                    if (!IsValid(neighbor, targetPos) || closed.Contains(neighbor))
                        continue;

                    var existing = open.FirstOrDefault(n => n.Equals(neighbor));

                    int newCost = current.Cost + 1;

                    if (existing != null)
                    {
                        if (newCost < existing.Cost)
                        {
                            existing.Cost = newCost;
                            existing.Parent = current;
                            existing.Value = existing.Cost + existing.Estimate;
                        }
                        continue;
                    }

                    neighbor.Cost = newCost;
                    neighbor.Parent = current;
                    neighbor.Estimate = Heuristic(neighbor, target);
                    neighbor.Value = neighbor.Cost + neighbor.Estimate;

                    open.Add(neighbor);
                }
            }

            return new List<Node>();
        }

        private List<Node> BuildPath(Node node)
        {
            var result = new List<Node>();

            while (node != null)
            {
                result.Add(node);
                node = node.Parent;
            }

            result.Reverse();
            return result;
        }

        private List<Node> GetNeighbors(Node node)
        {
            Vector2Int[] dirs =
            {
                Vector2Int.up,
                Vector2Int.right,
                Vector2Int.down,
                Vector2Int.left
            };

            return dirs
                .Select(d => new Node(node.Pos + d))
                .ToList();
        }

        private int Heuristic(Node a, Node b)
        {
            return Math.Abs(a.Pos.x - b.Pos.x) + Math.Abs(a.Pos.y - b.Pos.y);
        }
        private bool IsValid(Node node, Vector2Int targetPos)
        {
            bool inside =
                node.Pos.x >= 0 &&
                node.Pos.x < runtimeModel.RoMap.Width &&
                node.Pos.y >= 0 &&
                node.Pos.y < runtimeModel.RoMap.Height;

            if (!inside)
                return false;

            if (node.Pos == targetPos)
                return true;

            return runtimeModel.IsTileWalkable(node.Pos);
        }
    }
}