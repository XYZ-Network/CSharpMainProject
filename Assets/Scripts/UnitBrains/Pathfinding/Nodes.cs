using System;
using UnityEngine;

public class Nodes 
{
   public Vector2Int Position;

    public int Cost = 10;
    public int Estimate;
    public int Value;

    public Nodes Parent;

    public Nodes(Vector2Int position)
    {
        Position = position;
    }


    public void CalculateEstimate(int targetX, int targetY)
    {
       Estimate = Math.Abs(Position.x - targetX) + Math.Abs(Position.y - targetY);
    }

    public void CalculateValue()
    {
        Value = Cost + Estimate;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Nodes node)
            return false;

        return Position.x == node.Position.x && Position.y == node.Position.y;
    }
}
