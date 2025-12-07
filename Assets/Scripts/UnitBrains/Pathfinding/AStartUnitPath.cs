using Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Pathfinding;
using UnityEngine;
using Utilities;
using static UnityEditor.PlayerSettings;

public class AStarUnitPath : BaseUnitPath
{
    //private const int MaxLength = 100;
    private Vector2Int[] _shift = 
    {
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
    };
    public AStarUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint) : base(runtimeModel, startPoint, endPoint)
    {
    }

    protected override void Calculate()
    {
        path = FindPath().ToArray();
    }

    private List<Vector2Int> FindPath()
    {
        CellClass startCell = new CellClass(startPoint, runtimeModel);
        CellClass targetCell = new CellClass(endPoint, runtimeModel);

        List<CellClass> openList = new List<CellClass> { startCell };
        List<CellClass> closeList = new List<CellClass>();

        while (openList.Count > 0)
        {
            CellClass currentCell = openList[0];
            foreach (CellClass cell in openList)
            {
                if (cell.Value < currentCell.Value)
                    currentCell = cell;
            }
            openList.Remove(currentCell);
            closeList.Add(currentCell);

            if (currentCell._cords == targetCell._cords)
            {
                List<Vector2Int> path = new List<Vector2Int>();

                while (currentCell != null)
                {
                    path.Add(currentCell._cords);
                    currentCell = currentCell.Parent;
                }
                path.Reverse();
                return path;
            }
            for (int i = 0; i < _shift.Length; i++)
            {
                Vector2Int newCords = currentCell._cords + _shift[i];

                if (runtimeModel.IsTileWalkable(newCords) || newCords == targetCell._cords || runtimeModel.RoUnits.Any(u => u.Pos == newCords))
                {
                    CellClass nei = new CellClass(newCords, runtimeModel);

                    if (closeList.Contains(nei) || openList.Contains(nei))
                        continue;

                    nei.Parent = currentCell;
                    nei.CalculateEstimate(targetCell._cords);
                    nei.CalculateValue();

                    openList.Add(nei);
                }
            }
        }
        return null;
    }
}
