using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UnitBrains.Player
{
    public class Point
    {
        private Vector2Int toLeft = new Vector2Int(0, -1);
        private Vector2Int toRight = new Vector2Int(0, 1);
        private Vector2Int toDown = new Vector2Int(1, 0);
        private Vector2Int toUp = new Vector2Int(-1, 0);

        public int X;
        public int Y;
        public int Cost = 10;
        public int Estimate;
        public int Value;
        public Point Parent;

        public Point(int x, int y)
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
            if (obj is not Point point)
                return false;

            return X == point.X && Y == point.Y;
        }
    }
}
