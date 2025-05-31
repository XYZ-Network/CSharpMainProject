using Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Pathfinding;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;

public class AStarPath : BaseUnitPath
{
    private int[] dx = { -1, 0, 1, 0 };
    private int[] dy = { 0, 1, 0, -1 };
    private int maxLength => runtimeModel.RoMap.Width * runtimeModel.RoMap.Height;

    public AStarPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint) : base(runtimeModel, startPoint, endPoint)
    {
    }

    protected override void Calculate()
    {
        List<Vector2Int> pathList = FindPath();
   
        path = pathList.ToArray();
    }

    public List<Vector2Int> FindPath()
    {
        AStarNode startNode = new AStarNode(startPoint.x, startPoint.y);
        AStarNode targetNode = new AStarNode(endPoint.x, endPoint.y);

        List<AStarNode> openList = new List<AStarNode>() { startNode };
        List<AStarNode> closedList = new List<AStarNode>();

        while (openList.Count > 0)
        {
            AStarNode currentNode = openList[0];

            foreach (AStarNode node in openList)
            {
                if (node.Value < currentNode.Value) currentNode = node;
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            if (currentNode.x == targetNode.x && currentNode.y == targetNode.y || closedList.Count > maxLength)
            //if (currentNode.x == targetNode.x && currentNode.y == targetNode.y || closedList.Count > 10)
            {
                List<Vector2Int> newPath = new List<Vector2Int>();

                while (currentNode != null)
                {
                    newPath.Add(new Vector2Int(currentNode.x, currentNode.y));
                    currentNode = currentNode.Parent;
                }

                newPath.Reverse();
                return newPath;
            }
                for (int i = 0; i < dx.Length; i++)
                {
                    int newX = currentNode.x + dx[i];
                    int newY = currentNode.y + dy[i];

                    if (IsValid(new Vector2Int(newX, newY)))
                    {
                        AStarNode neighbor = new AStarNode(newX, newY);

                        if (openList.Contains(neighbor)) continue;

                        neighbor.Parent = currentNode;
                        neighbor.CalculateEstimate(targetNode.x, targetNode.y);
                        neighbor.CalculateValue();

                        openList.Add(neighbor);
                    }
                }

        }
        //return null;
        List<Vector2Int> noPath = new List<Vector2Int>();
        noPath.Add(new Vector2Int(startNode.x, startNode.y));
        return noPath;
    }

    private bool IsValid(Vector2Int cell)
    {
        bool isValidX = cell.x >= 0 && cell.x < runtimeModel.RoMap.Width;
        bool isValidY = cell.y >= 0 && cell.y < runtimeModel.RoMap.Height;

        return isValidX && isValidY && runtimeModel.IsTileWalkable(cell);
    }

}