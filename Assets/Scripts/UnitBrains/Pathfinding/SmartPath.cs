using System.Collections;
using System.Collections.Generic;
using Model;
using UnitBrains;
using UnitBrains.Pathfinding;
using UnityEngine;

public class SmartPath : BaseUnitPath
{
    public SmartPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int start, Vector2Int end) : base(runtimeModel, start,
        end)
    {
    }

    protected override void Calculate()
    {
        var openSet = new List<Vector2Int>();
        var closedSet = new HashSet<Vector2Int>();
        var gScore = new Dictionary<Vector2Int, int>();
        var fScore = new Dictionary<Vector2Int, int>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        openSet.Add(startPoint);

        gScore[startPoint] = 0;
        fScore[startPoint] = Heuristic(startPoint, endPoint);

        while (openSet.Count > 0)
        {
            Vector2Int currentPoint = openSet[0];

            foreach (var set in openSet)
            {
                if (fScore[set] < fScore[currentPoint])
                    currentPoint = set;
            }

            if (currentPoint == endPoint)
                break;

            openSet.Remove(currentPoint);
            closedSet.Add(currentPoint);

            foreach (var dir in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                Vector2Int neighbor = currentPoint + dir;
                
                var n = endPoint + dir;

                if (closedSet.Contains(neighbor))
                    continue;
                
                if (!runtimeModel.IsTileWalkable(neighbor) && neighbor != endPoint && neighbor != startPoint)
                    continue;

                int tentativeG = gScore[currentPoint] + 1;

                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = currentPoint;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, endPoint);

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }
        
        if (cameFrom.ContainsKey(endPoint))
        {
            var res = new List<Vector2Int>();
            Vector2Int currentPoint = endPoint;

            while (currentPoint != startPoint)
            {
                res.Add(currentPoint);
                currentPoint = cameFrom[currentPoint];
            }

            res.Add(startPoint);
            res.Reverse();
            path = res.ToArray();
        }
        else
            path = new[] { startPoint, startPoint };
    }

    int Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}