using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APathNode
{
    public int X;
    public int Y;
    public int Cost = 10;
    public int Estimate;
    public int Value;
    public APathNode Parent;
    public APathNode(Vector2Int v2)
    {
        X = v2.x;
        Y = v2.y;
    }
    public APathNode(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void CalculateEstimate(int targetX, int targetY)
    {
        Estimate = Math.Abs(X - targetX) + Math.Abs(Y - targetY);
    }
    public void CalculateValue()
    {
        Value = Math.Abs(Cost + Estimate);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not APathNode node)
            return false;
        return X == node.X && Y == node.Y;
    }
    public Vector2Int ReturnVector2()
    {
        return new Vector2Int(X, Y);
    }
}
