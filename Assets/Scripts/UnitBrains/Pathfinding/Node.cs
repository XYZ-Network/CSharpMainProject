using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UnitBrains.Pathfinding
{
    public class Node
    {
        public Vector2Int Position;
        public int Cost = 10;
        public int Estimate;
        public int Value;
        public Node Parent;

        public Node(Vector2Int pos)
        {
            Position = pos;
        }

        public void CalculateEstimate(int targetX, int targetY)
        {
            Estimate = Math.Abs(Position.x - targetX) + Math.Abs(Position.y - targetY);
        }

        public void CalculateValue()
        {
            Value = Cost + Estimate;
        }

        public override bool Equals(object obj)
        {
            if (obj is not Node node)
                return false;
            return Position.Equals(node.Position);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Position);
        }
    }
}