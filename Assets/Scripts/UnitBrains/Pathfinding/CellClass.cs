using Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Utilities;

public class CellClass
{
    public Vector2Int _cords;
    public int Cost;
    public int Estimate;
    public int Value;
    public CellClass Parent;

    public CellClass(Vector2Int cords, IReadOnlyRuntimeModel runtimeModel)
    {
        _cords = cords;
        Cost = runtimeModel.IsTileWalkable(cords) ? 10 : 30;
    }
    public void CalculateEstimate(Vector2Int target)
    {
        Estimate = Math.Abs(_cords.x - target.x) + Math.Abs(_cords.y - target.y);
    }
    public void CalculateValue()
    {
        Value = Cost + Estimate;
    }
    public override bool Equals(object? obj)
    {
        if (obj is not CellClass cell)
            return false;

        return _cords == cell._cords;
    }
}
