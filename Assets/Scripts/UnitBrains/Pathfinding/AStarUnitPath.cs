using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Pathfinding;
using UnityEngine;
using Utilities;

public class AStarUnitPath : BaseUnitPath
{
    private Vector2Int[] dxy = { new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1) };

    public AStarUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint) : base(runtimeModel, startPoint, endPoint)
    {
    }

    protected override void Calculate()
    {
        List<Vector2Int> result = FindPath();

        path = result.ToArray();
    }

    private List<Vector2Int> FindPath()
    {
        PathNode startPathNode = new PathNode(startPoint);
        PathNode targetPathNode = new PathNode(endPoint);

        List<PathNode> openList = new List<PathNode> { startPathNode };
        List<PathNode> closedList = new List<PathNode>();

        while (openList.Count > 0)
        {
            PathNode currentPathNode = openList[0];

            foreach (var PathNode in openList)
            {
                if (PathNode.Value < currentPathNode.Value)
                    currentPathNode = PathNode;
            }

            openList.Remove(currentPathNode);
            closedList.Add(currentPathNode);

            if (currentPathNode.Position == targetPathNode.Position)
            {
                List<Vector2Int> path = new List<Vector2Int>();

                while (currentPathNode != null)
                {
                    path.Add(currentPathNode.Position);
                    currentPathNode = currentPathNode.Parent;
                }

                path.Reverse();
                return path;
            }

            for (int i = 0; i < dxy.Length; i++)
            {
                Vector2Int newPos = currentPathNode.Position + dxy[i];

                if (runtimeModel.IsTileWalkable(newPos) || newPos == targetPathNode.Position || IsUnitAtPos(newPos))
                {

                    PathNode neighbor = new PathNode(newPos);
                    if (closedList.Contains(neighbor) || openList.Contains(neighbor))
                        continue;

                    neighbor.Parent = currentPathNode;
                    neighbor.CalculateEstimate(targetPathNode.Position);
                    neighbor.CalculateValue();

                    openList.Add(neighbor);
                }
            }
        }
        return new List<Vector2Int> { startPoint, startPoint };
    }

    private bool IsUnitAtPos(Vector2Int pos) =>
        runtimeModel.RoUnits.Any(u => u.Pos == pos);

}

public class PathNode
{
    private IReadOnlyRuntimeModel runtimeModel => ServiceLocator.Get<IReadOnlyRuntimeModel>();

    public Vector2Int Position;
    public int Cost;
    public int Estimate;
    public int Value;
    public PathNode Parent;

    public PathNode(Vector2Int position)
    {
        Position = position;
        Cost = runtimeModel.IsTileWalkable(Position) ? 10 : 30;
    }

    public void CalculateEstimate(Vector2Int target)
    {
        Estimate = Math.Abs(Position.x - target.x) + Math.Abs(Position.y - target.y);
    }
    public void CalculateValue()
    {
        Value = Cost + Estimate;
    }
    public override bool Equals(object? obj)
    {
        if (obj is not PathNode PathNode)
            return false;

        return Position.x == PathNode.Position.x && Position.y == PathNode.Position.y;
    }
}