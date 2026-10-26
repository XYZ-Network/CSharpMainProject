using Model;
using System.Collections;
using System.Collections.Generic;
using UnitBrains.Pathfinding;
using UnityEngine;

public class AStarUnitPath : BaseUnitPath
{
    private const int StepCost = 10;
    private const int MaxIterations = 20000;

    private static readonly int[] dx = { -1, 0, 1, 0 };
    private static readonly int[] dy = { 0, 1, 0, -1 };

    public AStarUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint)
        : base(runtimeModel, startPoint, endPoint) { }

    protected override void Calculate()
    {
        if (startPoint == endPoint)
        {
            path = new[] { startPoint };
            return;
        }

        // ВАЖНО: база противника непроходима — ищем ближайшую проходимую к ней.
        Vector2Int realGoal = FindNearestWalkable(endPoint);
        if (realGoal == startPoint)
        {
            path = new[] { startPoint };
            return;
        }

        var startNode = new Node(startPoint.x, startPoint.y);
        var targetNode = new Node(realGoal.x, realGoal.y);

        var openList = new List<Node> { startNode };
        var openSet = new HashSet<Vector2Int> { startPoint };
        var closedSet = new HashSet<Vector2Int>();

        int iterations = 0;

        while (openList.Count > 0)
        {
            if (iterations++ > MaxIterations)
            {
                Debug.LogWarning("A* iteration limit reached");
                path = new[] { startPoint };
                return;
            }

            // Ищем узел с минимальным Value
            int bestIdx = 0;
            for (int i = 1; i < openList.Count; i++)
                if (openList[i].Value < openList[bestIdx].Value)
                    bestIdx = i;

            var currentNode = openList[bestIdx];
            openList.RemoveAt(bestIdx);

            var currentPos = new Vector2Int(currentNode.X, currentNode.Y);
            openSet.Remove(currentPos);

            if (currentNode.X == targetNode.X && currentNode.Y == targetNode.Y)
            {
                path = BuildPath(currentNode);
                return;
            }

            if (!closedSet.Add(currentPos))
                continue;

            for (int i = 0; i < 4; i++)
            {
                int newX = currentNode.X + dx[i];
                int newY = currentNode.Y + dy[i];
                var neighborPos = new Vector2Int(newX, newY);

                if (!IsValid(newX, newY)) continue;
                if (closedSet.Contains(neighborPos)) continue;
                if (openSet.Contains(neighborPos)) continue;

                var neighbor = new Node(newX, newY) { Parent = currentNode };
                neighbor.CalculateEstimate(targetNode.X, targetNode.Y);
                neighbor.CalculateValue();

                openList.Add(neighbor);
                openSet.Add(neighborPos);
            }
        }

        // Путь не найден — стоим на месте
        path = new[] { startPoint };
    }

    private bool IsValid(int x, int y)
        => runtimeModel.IsTileWalkable(new Vector2Int(x, y));

    /// <summary>
    /// BFS: ближайшая проходимая клетка к target.
    /// Нужно из-за непроходимой базы противника.
    /// </summary>
    private Vector2Int FindNearestWalkable(Vector2Int target)
    {
        if (IsValid(target.x, target.y))
            return target;

        var visited = new HashSet<Vector2Int> { target };
        var queue = new Queue<Vector2Int>();
        queue.Enqueue(target);

        int guard = 0;
        while (queue.Count > 0 && guard++ < 2000)
        {
            var cur = queue.Dequeue();
            for (int i = 0; i < 4; i++)
            {
                var n = new Vector2Int(cur.x + dx[i], cur.y + dy[i]);
                if (!visited.Add(n)) continue;

                if (IsValid(n.x, n.y))
                    return n;

                queue.Enqueue(n);
            }
        }
        return target;
    }

    private Vector2Int[] BuildPath(Node node)
    {
        var result = new List<Vector2Int>();
        while (node != null)
        {
            result.Add(new Vector2Int(node.X, node.Y));
            node = node.Parent;
        }
        result.Reverse();
        return result.ToArray();
    }

    private class Node
    {
        public int X, Y;
        public int Cost = StepCost;
        public int Estimate;
        public int Value;
        public Node Parent;

        public Node(int x, int y) { X = x; Y = y; }

        public void CalculateEstimate(int tx, int ty)
            => Estimate = (Mathf.Abs(X - tx) + Mathf.Abs(Y - ty)) * StepCost;

        public void CalculateValue() => Value = Cost + Estimate;
    }
}
