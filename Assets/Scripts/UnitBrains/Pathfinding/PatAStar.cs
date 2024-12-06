using Model;
using PlasticGui;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Pathfinding;
using UnityEngine;
using UnityEngine.UIElements;

public class PatAStar : BaseUnitPath
{
    public PatAStar(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint) : base(runtimeModel, startPoint, endPoint)
    {
    }

    private List <Vector2Int> _vectorsXY =  new List<Vector2Int> { Vector2Int.up, Vector2Int.down, Vector2Int.right, Vector2Int.left };

    protected override void Calculate()
    {
        if (FindPath() is not null)
        {        
            path = FindPath().ToArray();
            
        }
        else
        {
            path = null;
        }

        if (path == null)
        {
            path = new Vector2Int[] { startPoint };
        }
    }

    private List<Vector2Int> FindPath()
    {
        NodePath start = new NodePath(startPoint);
        NodePath end = new NodePath(endPoint);

        List<NodePath> openList = new List<NodePath>() { start };
        List<NodePath> closeList = new List<NodePath>();

        while (openList.Count > 0)
        {
            NodePath currentNode = openList[0];

            foreach (NodePath node in openList)
            {
                if (node.value < currentNode.value)
                {
                    currentNode = node;
                }
            }

            openList.Remove(currentNode);
            closeList.Add(currentNode);

            if (currentNode.position.x == end.position.x && currentNode.position.y == end.position.y)
            {
                return ConvertNodeToPos(currentNode);
            }

            for (int i = 0; i < _vectorsXY.Count; i++)
            {
                Vector2Int newPosition = currentNode.position + _vectorsXY[i];
                if (!IsValid(newPosition) && newPosition != endPoint && !IsBlockedByEnemy(newPosition))
                {
                    continue;
                }

                    
                NodePath neighbor = new NodePath(newPosition);
                
                if (closeList.Contains(neighbor))
                {
                    continue;
                }

                neighbor.parent = currentNode;
                neighbor.CalculateEstimate(end.position);
                
                neighbor.CalculateValue();
                if (openList.Contains(neighbor))
                {
                    continue;
                }
                openList.Add(neighbor);
                
            }
        }

        return null;
    }

    private List<Vector2Int> ConvertNodeToPos(NodePath currentNode)
    {
        List<NodePath> path = new List<NodePath>();

        while (currentNode != null) 
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        
        return path.Select(node => node.position).ToList();
    }

    private bool IsValid(Vector2Int position )
    {
        Vector2Int mapSize = new Vector2Int(runtimeModel.RoMap.Width, runtimeModel.RoMap.Height);
        bool containsX = position.x >= 0 && position.x < mapSize.x;
        bool containsY = position.y >= 0 && position.y < mapSize.y;

        return containsX && containsY && runtimeModel.IsTileWalkable(position);
    }

    private bool IsBlockedByEnemy(Vector2Int tempPos)
    {
        var botPos = runtimeModel.RoBotUnits.Select(u => u.Pos).Where(u => u == tempPos);
        return botPos.Any();
    }
}

