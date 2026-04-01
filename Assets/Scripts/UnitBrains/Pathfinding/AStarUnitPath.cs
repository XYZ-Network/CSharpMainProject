using System.Collections.Generic;
using Model;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    public class AStarUnitPath : BaseUnitPath
    {
        public AStarUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint)
            : base(runtimeModel, startPoint, endPoint)
        {
        }

        protected override void Calculate()
        {
            var openSet = new List<Node>();
            var closedSet = new HashSet<Vector2Int>();
            var nodeMap = new Dictionary<Vector2Int, Node>();

            var startNode = new Node(startPoint, 0, Heuristic(startPoint, endPoint), null);
            openSet.Add(startNode);
            nodeMap[startPoint] = startNode;

            while (openSet.Count > 0)
            {
                var current = GetLowestFCost(openSet);

                if (current.Position == endPoint)
                {
                    path = ReconstructPath(current);
                    return;
                }

                openSet.Remove(current);
                closedSet.Add(current.Position);

                foreach (var neighborPos in GetNeighbors(current.Position))
                {
                    if (closedSet.Contains(neighborPos))
                        continue;

                    if (neighborPos != endPoint && !runtimeModel.IsTileWalkable(neighborPos))
                        continue;

                    var tentativeG = current.GCost + 1;

                    if (!nodeMap.TryGetValue(neighborPos, out var neighborNode))
                    {
                        neighborNode = new Node(neighborPos, tentativeG, Heuristic(neighborPos, endPoint), current);
                        openSet.Add(neighborNode);
                        nodeMap[neighborPos] = neighborNode;
                    }
                    else if (tentativeG < neighborNode.GCost)
                    {
                        neighborNode.GCost = tentativeG;
                        neighborNode.Parent = current;
                        if (!openSet.Contains(neighborNode))
                            openSet.Add(neighborNode);
                    }
                }
            }

            // No path found — stay in place
            path = new[] { startPoint };
        }

        private static IEnumerable<Vector2Int> GetNeighbors(Vector2Int pos)
        {
            yield return pos + Vector2Int.up;
            yield return pos + Vector2Int.down;
            yield return pos + Vector2Int.left;
            yield return pos + Vector2Int.right;
        }

        private static int Heuristic(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        private static Node GetLowestFCost(List<Node> nodes)
        {
            var best = nodes[0];
            for (int i = 1; i < nodes.Count; i++)
            {
                if (nodes[i].FCost < best.FCost ||
                    (nodes[i].FCost == best.FCost && nodes[i].HCost < best.HCost))
                    best = nodes[i];
            }
            return best;
        }

        private static Vector2Int[] ReconstructPath(Node endNode)
        {
            var result = new List<Vector2Int>();
            var current = endNode;
            while (current != null)
            {
                result.Add(current.Position);
                current = current.Parent;
            }
            result.Reverse();
            return result.ToArray();
        }

        private class Node
        {
            public readonly Vector2Int Position;
            public int GCost;
            public readonly int HCost;
            public Node Parent;
            public int FCost => GCost + HCost;

            public Node(Vector2Int position, int gCost, int hCost, Node parent)
            {
                Position = position;
                GCost = gCost;
                HCost = hCost;
                Parent = parent;
            }
        }
    }
}
