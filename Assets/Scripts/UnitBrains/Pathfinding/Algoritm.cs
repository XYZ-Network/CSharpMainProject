using System;
using System.Collections.Generic;
using Model;
using UnitBrains.Pathfinding;
using UnityEngine;
using Unit = Model.Runtime.Unit;

public class Algoritm : BaseUnitPath
{

    private int[] dx = { -1, 0, 1, 0 };
    private int[] dy = { 0, 1, 0, -1 };
    public int calc;

    // Максимальное количество итераций перед считанием, что алгоритм застрял
    private const int MaxIterations = 1000;
    private int iterationCount = 0;

    public Algoritm(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint)
        : base(runtimeModel, startPoint, endPoint)
    {
    }

    protected override void Calculate()
    {
        Nodes startNode = new Nodes(startPoint);
        Nodes targetNode = new Nodes(endPoint);

        List<Nodes> openList = new List<Nodes> { startNode };
        List<Nodes> clodesList = new List<Nodes>();

        while (openList.Count > 0)
        {
            Nodes currentNode = openList[0];
            Debug.Log("Current node position: " + currentNode.Position);
            Debug.Log(endPoint);

            foreach (var node in openList)
            {
                if (node.Value < currentNode.Value)
                    currentNode = node;
            }

            openList.Remove(currentNode);
            Debug.Log("Removed from open list: " + currentNode.Position);
            clodesList.Add(currentNode);
            Debug.Log("Added node to closed list: " + currentNode.Position);

            if (currentNode.Position == endPoint)
            {
                if (!IsValid(endPoint.x, endPoint.y))
                {
                    Debug.Log("Конечная точка непроходима " + endPoint.x + endPoint.y);
                    continue;
                }
                else
                {
                    // Если конечная точка проходима, завершаем алгоритм и возвращаем путь
                    Debug.Log("Конечная точка проходима " + endPoint.x + endPoint.y);
                    List<Nodes> path = new List<Nodes>();
                    while (currentNode != null)
                    {
                        path.Add(currentNode);
                        Debug.Log("Создали путь " + currentNode);
                        currentNode = currentNode.Parent;
                    }

                    path.Reverse();
                    Debug.Log("вернули путь" + currentNode);
                    return;
                }
            }

            iterationCount++;

            if (iterationCount > MaxIterations)
            {
                throw new Exception("Pathfinding algorithm is stuck.");
            }

            for (int i = 0; i < dx.Length; i++)
            {
                int newX = currentNode.Position.x + dx[i];
                int newY = currentNode.Position.y + dy[i];

                if (IsValid(newX, newY))
                {
                    Debug.Log("Tile at (" + newX + ", " + newY + ") is walkable.");

                    Nodes neighbor = new Nodes(new Vector2Int(newX, newY));

                    if (clodesList.Contains(neighbor))
                        continue;

                    neighbor.Parent = currentNode;
                    neighbor.CalculateEstimate(endPoint.x, endPoint.y);
                    neighbor.CalculateValue();

                    openList.Add(neighbor);
                    Debug.Log("Added neighbor node to open list: " + neighbor.Position);
                }
                else
                {
                    Nodes neighborclose = new Nodes(new Vector2Int(newX, newY));
                    clodesList.Add(neighborclose);
                    Debug.Log("Tile at (" + newX + ", " + newY + ") is not walkable.");
                }
            }
        }

    }


    private bool IsValid(int x, int y)
    {
        bool containsX = x >= 0 && x < runtimeModel.RoMap.Width;
        bool containsY = y >= 0 && y < runtimeModel.RoMap.Height;
        return containsX && containsY && (runtimeModel.IsTileWalkable(new Vector2Int(x, y)));

    }
}
