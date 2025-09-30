using Codice.Client.Common.GameUI;
using Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Pathfinding;
using UnityEngine;
using Utilities;
using static UnityEngine.GraphicsBuffer;

public class BrilliantUnitPath : BaseUnitPath
{
    private Vector2Int[] dxy = { new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1) };

    public BrilliantUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint) : base(runtimeModel, startPoint, endPoint)
    {
    }

    protected override void Calculate()
    {
        List<Vector2Int> result = FindPath();

        path = result.ToArray();
    }

    private List<Vector2Int> FindPath()
    {
        PathNode startNode = new PathNode(startPoint);
        PathNode targetNode = new PathNode(endPoint);

        List<PathNode> openList = new List<PathNode> { startNode };
        List<PathNode> closedList = new List<PathNode>();

        while (openList.Count > 0)
        {
            PathNode currentNode = openList[0];

            foreach (var node in openList) 
            {
                if (node.Value < currentNode.Value)
                    currentNode = node;
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            if (currentNode.Position == targetNode.Position)
            {
                List<Vector2Int> path = new List<Vector2Int>();

                while (currentNode != null)
                {
                    path.Add(currentNode.Position);
                    currentNode = currentNode.Parent;
                }

                path.Reverse();
                return path;
            }

            for (int i = 0; i < dxy.Length; i++)
            {
                Vector2Int newPos = currentNode.Position + dxy[i];

                if (runtimeModel.IsTileWalkable(newPos) || newPos == targetNode.Position || IsUnitAtPos(newPos))
                {

                    PathNode neighbor = new PathNode(newPos);
                    if (closedList.Contains(neighbor) || openList.Contains(neighbor))
                        continue;

                    neighbor.Parent = currentNode;
                    neighbor.CalculateEstimate(targetNode.Position);
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
