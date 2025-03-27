using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Model;
using UnitBrains.Pathfinding;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class AStarPath : BaseUnitPath
{
    private Vector2Int[] directions = { Vector2Int.left, Vector2Int.up, Vector2Int.right, Vector2Int.down };
    private const int MaxLength = 200;

    public AStarPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint) : base(runtimeModel, startPoint, endPoint)
    {
    }

    protected override void Calculate()
    {
        var currentPoint = startPoint;
        var result = new List<Vector2Int>();
        var smartPath = FindPath();
        if (smartPath != null)
        {
            foreach (Cell cell in smartPath)
            {
                result.Add(cell.Position);
            }
        }
        else
        {
            result.Add(currentPoint);
        }
        path = result.ToArray();
    }

    public List<Cell> FindPath()
    {
        Cell startCell = new Cell(startPoint);
        Cell targetCell = new Cell(endPoint);

        List<Cell> openList = new List<Cell> { startCell };
        List<Cell> closedList = new List<Cell>();

        while (openList.Count > 0)
        {
            Cell currentCell = openList[0];

            foreach (var cell in openList)
            {
                if (cell.Value < currentCell.Value)
                    currentCell = cell;
            }

            openList.Remove(currentCell);
            closedList.Add(currentCell);

            if (currentCell.Equals(targetCell) || closedList.Count > MaxLength)
            {
                List<Cell> path = new List<Cell>();

                while (currentCell != null)
                {
                    path.Add(currentCell);
                    currentCell = currentCell.Parent;
                }

                path.Reverse();
                return path;
            }

            foreach (var direction in directions)
            {
                Vector2Int newPos = currentCell.Position + direction;

                if (IsValid(newPos))
                {
                    Cell neighbor = new Cell(newPos);

                    if (closedList.Contains(neighbor))
                        continue;

                    neighbor.Parent = currentCell;
                    neighbor.CalculateEstimate(targetCell.Position);
                    neighbor.CalculateValue();

                    openList.Add(neighbor);
                }
            }
        }

        return null;
    }

    private bool IsValid(Vector2Int pos)
    {
        bool isTileWalkable = runtimeModel.IsTileWalkable(pos);
        return isTileWalkable || pos == endPoint;
    }
}

public class Cell
{
    public Vector2Int Position;
    public int Cost = 10;
    public int Estimate;
    public int Value;
    public Cell Parent;

    public Cell(Vector2Int position)
    {
        Position = position;
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
        if (obj is not Cell cell)
            return false;

        return Position.x == cell.Position.x && Position.y == cell.Position.y;
    }
}
