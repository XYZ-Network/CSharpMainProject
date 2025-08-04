using System;

namespace Assets.Scripts.UnitBrains.Pathfinding
{
    public class PathNode
    {
        public int X;
        public int Y;
        public int Cost = 3;
        public int Estimate;
        public int Value;
        public PathNode Parent;
        public PathNode(int x, int y)
        {
            X = x;
            Y = y;
            Parent = null;
        }
        public void CalculateEstimate(int targetX, int targetY)
        {
            Estimate = Math.Abs(X - targetX) + Math.Abs(Y - targetY);
        }
        public void CalculateValue()
        {
            Value = Cost + Estimate;
        }
        public override bool Equals(object obj)
        {
            if (obj is not PathNode node)
                return false;

            return X == node.X && Y == node.Y;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}