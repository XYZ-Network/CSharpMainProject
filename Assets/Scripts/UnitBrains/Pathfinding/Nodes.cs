//using System;
//using UnityEngine;

//public class Nodes
//{
//    public Vector2Int Position;

//    public int Cost = 10;
//    public int Estimate;
//    public int Value;

//    public Nodes Parent;

//    public Nodes(Vector2Int position)
//    {
//        Position = position;
//    }

//    public override bool Equals(object obj)
//    {
//        return obj is Nodes node &&
//               Position == node.Position;
//    }

//    public override int GetHashCode()
//    {
//        return HashCode.Combine(Position);
//    }

//    public void CalculateEstimate(Nodes targetPosition)
//    {
//        Estimate = Math.Abs(Position.x - targetPosition.Position.x) + Math.Abs(Position.y - targetPosition.Position.y);
//    }

//    public void CalculateValue()
//    {
//        Value = Cost + Estimate;
//    }

//}