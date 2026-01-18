using System;
using UnityEngine;

namespace UnitBrains.Pathfinding
{

    public class Node
    {

        public int X { get; private set; }
        public int Y { get; private set; }

        public int Cost { get; private set; } = 10;

        public int Estimate { get; private set; }
        public int Value { get; private set; }

        public Node Parent { get; set; }

        public Node(int x, int y)
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
            Value = Cost + Estimate;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Node point)
                return false;

            return X == point.X && Y == point.Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }
}