using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Model;
using Utilities;

public class PathNode          // : MonoBehaviour
{
    private IReadOnlyRuntimeModel runtimeModel => ServiceLocator.Get<IReadOnlyRuntimeModel>();

    public Vector2Int Position;
    public int Cost;
    public int Estimate;
    public int Value;
    public PathNode Parent;

    public PathNode(Vector2Int position)
    {
        Position = position;
        Cost = runtimeModel.IsTileWalkable(Position) ? 10 : 30;
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
        if (obj is not PathNode node)
            return false;

        return Position.x == node.Position.x && Position.y == node.Position.y;
    }

    
}
