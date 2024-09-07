using Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Pathfinding;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;

public class AStarPathFinding : BaseUnitPath
{
    private int[] dx = { -1, 0, 1, 0 };
    private int[] dy = { 0, 1, 0, -1 };
    private int maxLength => runtimeModel.RoMap.Width * runtimeModel.RoMap.Height;

    public AStarPathFinding(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint) : base(runtimeModel, startPoint, endPoint)
    {       
    }

    protected override void Calculate()
    {
        List<Vector2Int> pathList = FindPath();
        
        path = pathList.ToArray();
       
    }

    public List<Vector2Int> FindPath()
    {
        Nodes startNode = new Nodes(startPoint.x, startPoint.y);
        Nodes targetNode = new Nodes(endPoint.x, endPoint.y);

        List<Nodes> openList = new List<Nodes>() { startNode};
        List<Nodes> closedList = new List<Nodes>();

        while(openList.Count > 0 )
        {
            Nodes currentNode = openList[0];
            
            foreach(Nodes node in openList)
            {                              
                if (node.value < currentNode.value) currentNode = node;              
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            if(currentNode.x == targetNode.x && currentNode.y == targetNode.y || closedList.Count > maxLength)
            {               
                List<Vector2Int> newPath = new List<Vector2Int>();
                
                while (currentNode != null)
                {
                    newPath.Add(new Vector2Int(currentNode.x, currentNode.y));
                    currentNode = currentNode.parent;
                }

                newPath.Reverse();
                return newPath;
            }

            for(int i = 0; i < dx.Length; i++)
            {
                int newX = currentNode.x + dx[i];
                int newY = currentNode.y + dy[i];
                
                if(IsValid(new Vector2Int(newX, newY)))
                {
                    Nodes neighbor = new Nodes(newX, newY);

                    if (openList.Contains(neighbor)) continue;

                    neighbor.parent = currentNode;
                    neighbor.CalculateEstimate(targetNode.x, targetNode.y);
                    neighbor.CalculateValue();

                    openList.Add(neighbor);                    
                }
            }

        }
        return null;
    }

    private bool IsValid(Vector2Int cell)
    {
        bool isValidX = cell.x >=0 && cell.x < runtimeModel.RoMap.Width;
        bool isValidY = cell.y >=0 && cell.y < runtimeModel.RoMap.Height;

        return isValidX && isValidY && runtimeModel.IsTileWalkable(cell);
    }

}


