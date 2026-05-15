using System.Collections.Generic;
using Model;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    public class AStarUnitPath : BaseUnitPath
    {
        private static readonly Vector2Int[] Directions =
        {
            new Vector2Int(0, 1),
            new Vector2Int(1, 0),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0),

            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, -1)
        };

        private const int StraightCost = 10;
        private const int DiagonalCost = 14;
        private const int MaxGoalRadius = 30;

        public AStarUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint)
            : base(runtimeModel, startPoint, endPoint)
        {
        }

        protected override void Calculate()
        {
            for (int radius = 0; radius <= MaxGoalRadius; radius++)
            {
                HashSet<Vector2Int> goalCells = GetGoalCellsAtRadius(endPoint, radius);

                if (goalCells.Count == 0)
                    continue;

                if (goalCells.Contains(startPoint))
                {
                    path = new[] { startPoint, startPoint };
                    return;
                }

                if (TryBuildPath(goalCells, out Vector2Int[] resultPath))
                {
                    path = resultPath;
                    return;
                }
            }

            Debug.LogWarning($"Path not found from {startPoint} to {endPoint}");
            path = new[] { startPoint, startPoint };
        }

        private bool TryBuildPath(HashSet<Vector2Int> goalCells, out Vector2Int[] resultPath)
        {
            var openList = new List<Node>();
            var closedSet = new HashSet<Vector2Int>();
            var allNodes = new Dictionary<Vector2Int, Node>();

            var startNode = new Node(startPoint)
            {
                Cost = 0,
                Estimate = GetEstimate(startPoint, goalCells)
            };
            startNode.RefreshValue();

            openList.Add(startNode);
            allNodes[startPoint] = startNode;

            while (openList.Count > 0)
            {
                Node currentNode = openList[0];

                foreach (var node in openList)
                {
                    if (node.Value < currentNode.Value ||
                        (node.Value == currentNode.Value && node.Estimate < currentNode.Estimate))
                    {
                        currentNode = node;
                    }
                }

                openList.Remove(currentNode);
                closedSet.Add(currentNode.Position);

                if (goalCells.Contains(currentNode.Position))
                {
                    resultPath = BuildPath(currentNode).ToArray();
                    return true;
                }

                foreach (var direction in Directions)
                {
                    Vector2Int nextPos = currentNode.Position + direction;

                    if (!IsPathCellAvailable(nextPos))
                        continue;

                    if (closedSet.Contains(nextPos))
                        continue;

                    int stepCost = IsDiagonal(direction) ? DiagonalCost : StraightCost;
                    int nextCost = currentNode.Cost + stepCost;

                    if (!allNodes.TryGetValue(nextPos, out Node nextNode))
                    {
                        nextNode = new Node(nextPos);
                        allNodes.Add(nextPos, nextNode);
                    }

                    if (openList.Contains(nextNode) && nextCost >= nextNode.Cost)
                        continue;

                    nextNode.Parent = currentNode;
                    nextNode.Cost = nextCost;
                    nextNode.Estimate = GetEstimate(nextPos, goalCells);
                    nextNode.RefreshValue();

                    if (!openList.Contains(nextNode))
                        openList.Add(nextNode);
                }
            }

            resultPath = null;
            return false;
        }

        private HashSet<Vector2Int> GetGoalCellsAtRadius(Vector2Int center, int radius)
        {
            var result = new HashSet<Vector2Int>();

            if (radius == 0)
            {
                if (IsPathCellAvailable(center))
                    result.Add(center);

                return result;
            }

            for (int x = center.x - radius; x <= center.x + radius; x++)
            {
                for (int y = center.y - radius; y <= center.y + radius; y++)
                {
                    Vector2Int cell = new Vector2Int(x, y);

                    int chebyshev = Mathf.Max(
                        Mathf.Abs(cell.x - center.x),
                        Mathf.Abs(cell.y - center.y));

                    if (chebyshev != radius)
                        continue;

                    if (!IsPathCellAvailable(cell))
                        continue;

                    result.Add(cell);
                }
            }

            return result;
        }

        private bool IsPathCellAvailable(Vector2Int cell)
        {
            if (runtimeModel.IsTileWalkable(cell))
                return true;

            foreach (var unit in runtimeModel.RoUnits)
            {
                if (unit.Pos == cell)
                    return true;
            }

            return false;
        }

        private static List<Vector2Int> BuildPath(Node endNode)
        {
            var result = new List<Vector2Int>();
            Node current = endNode;

            while (current != null)
            {
                result.Add(current.Position);
                current = current.Parent;
            }

            result.Reverse();
            return result;
        }

        private static int GetEstimate(Vector2Int from, HashSet<Vector2Int> goalCells)
        {
            int best = int.MaxValue;

            foreach (var goal in goalCells)
            {
                int dx = Mathf.Abs(from.x - goal.x);
                int dy = Mathf.Abs(from.y - goal.y);

                int diagonal = Mathf.Min(dx, dy);
                int straight = Mathf.Abs(dx - dy);

                int estimate = diagonal * DiagonalCost + straight * StraightCost;

                if (estimate < best)
                    best = estimate;
            }

            return best;
        }

        private static bool IsDiagonal(Vector2Int direction)
        {
            return direction.x != 0 && direction.y != 0;
        }

        private sealed class Node
        {
            public Vector2Int Position;
            public int Cost = int.MaxValue;
            public int Estimate;
            public int Value;
            public Node Parent;

            public Node(Vector2Int position)
            {
                Position = position;
            }

            public void RefreshValue()
            {
                Value = Cost + Estimate;
            }

            public override bool Equals(object obj)
            {
                return obj is Node other && other.Position == Position;
            }

            public override int GetHashCode()
            {
                return Position.GetHashCode();
            }
        }
    }
}