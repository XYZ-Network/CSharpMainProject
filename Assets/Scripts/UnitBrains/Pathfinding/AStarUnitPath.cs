using Model;
using System.Collections.Generic;
using System.IO;
using System;
using UnitBrains.Pathfinding;
using UnityEngine;
using Model.Runtime;
using System.Linq;

public class AStarUnitPath : BaseUnitPath
{
    private Vector2Int _startPoint;
    private Vector2Int _endPoint;
    private Vector2Int _prevPos;
    private IReadOnlyRuntimeModel _runTimeModel;
    private int[] dx = { 1, 0, -1, 0};
    private int[] dy = { 0, 1, 0, -1};

    public AStarUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint,Vector2Int prevPos) : base(runtimeModel, startPoint, endPoint)
    {
        _startPoint = startPoint;
        _endPoint = endPoint;
        _runTimeModel = runtimeModel;
        _prevPos = prevPos;
    }


    protected override void Calculate()
    {

        //¬се вершины в которые можно пойти
        List<Vector2Int> openList = new List<Vector2Int>() { _startPoint };
        // все вершины по которым прошли
        HashSet<Vector2Int> closedList = new HashSet<Vector2Int>();
        var result = new List<Vector2Int> { _startPoint };
        while (openList.Count > 0)
        {
            Vector2Int currentPoint = openList[0];

            foreach (var point in openList)
            {
                if (CalculateValue(point) < CalculateValue(currentPoint) || currentPoint == _prevPos || currentPoint == startPoint)
                {
                    currentPoint = point;
                }
            }
            openList.Remove(currentPoint);
            closedList.Add(currentPoint);
            //ѕроверка на конец пути
            if (currentPoint == _endPoint)
            {
                path = closedList.ToArray();
                return;
            }
            //–ассчет следующей ноды
            bool newNodeAdded = false;
            for (int i = 0; i < dx.Length; i++)
            {
                Vector2Int newPoint = new Vector2Int(currentPoint.x + dx[i], currentPoint.y + dy[i]);

                // ѕровер€ем, не находитс€ ли точка в закрытом списке
                if (closedList.Contains(newPoint))
                {
                    continue;
                }

                // ѕровер€ем, проходима ли точка или €вл€етс€ конечной точкой
                if (_runTimeModel.IsTileWalkable(newPoint) || (newPoint == _endPoint))
                {
                    if (!openList.Contains(newPoint))
                    {
                        openList.Add(newPoint);
                        newNodeAdded = true;
                    }
                }
                if (isUnitOnTile(newPoint))
                {
                    continue;
                }

            }
            if (!newNodeAdded)
            {
                
                path = closedList.ToArray();
                return;
            }
            
        }
    }

    public void CalculateNewPoints(Vector2Int currentPoint, List<Vector2Int> openList, HashSet<Vector2Int> closedList, int depth = 0)
    {
        // ќграничиваем глубину рекурсии дл€ предотвращени€ бесконечных вызовов
        int maxDepth = 1;
        if (depth > maxDepth)
        { 
            return;
        }

        for (int i = 0; i < dx.Length; i++)
        {
            Vector2Int newPoint = new Vector2Int(currentPoint.x + dx[i], currentPoint.y + dy[i]);

            // ѕровер€ем, не находитс€ ли точка в закрытом списке
            if (closedList.Contains(newPoint) || openList.Contains(newPoint))
            {
                continue;
            }

            // ѕровер€ем, проходима ли точка или €вл€етс€ конечной точкой
            if (_runTimeModel.IsTileWalkable(newPoint) || (newPoint == _endPoint))
            {
                if (!openList.Contains(newPoint))
                {
                    openList.Add(newPoint);
                }
            }
            if (!runtimeModel.IsTileWalkable(newPoint))
            {
                CalculateNewPoints(newPoint, openList, closedList,depth + 1);
            }

        }
    }

    public bool isUnitOnTile(Vector2Int point)
    {
        foreach (var unit in _runTimeModel.RoUnits)
        {
            if (unit.Pos == point)
            {
                return true;
            }
        }
        return false;
    }
    public int CalculateEstimate(int targetX, int targetY, Vector2Int point)
    {
        return Math.Abs(point.x - targetX) + Math.Abs(point.y - targetY);
    }
    public int CalculateValue(Vector2Int point, int Cost = 10)
    {
        return Math.Abs(Cost + CalculateEstimate(_endPoint.x, _endPoint.y, point)) ;
    }

    public Vector2Int GetPrevPos()
    {
        return _startPoint;
    }
}