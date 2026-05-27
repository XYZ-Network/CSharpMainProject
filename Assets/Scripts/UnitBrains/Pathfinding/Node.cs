using System;

namespace UnitBrains.Pathfinding
{
    public class Node
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int G { get; set; }
        public int H { get; set; }
        public int Value { get; set; }
        public Node Parent { get; set; }

        public Node(int x, int y)
        {
            X = x;
            Y = y;
            G = 0;
            H = 0;
            Value = 0;
            Parent = null;
        }

        public void CalculateEstimate(int targetX, int targetY)
        {
            H = Math.Abs(targetX - X) + Math.Abs(targetY - Y);
        }

        public void CalculateValue()
        {
            Value = G + H;
        }

        public override bool Equals(object obj)
        {
            if (obj is Node other)
                return X == other.X && Y == other.Y;
            return false;
        }

        public override int GetHashCode()
        {
            return (X, Y).GetHashCode();
        }

        public override string ToString()
        {
            return $"({X}, {Y}) G={G} H={H} F={Value}";
        }
    }
}