
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UnitBrains.Pathfinding
{
    public class Point
    {
        public Vector2Int coordinates;
        public int StepCost = 10;
        public int SecondStepCost = 14;
        public int Estimate;
        public int Value;
        public Point parent;

        public Point(int x, int y)
        {
            this.coordinates = coordinates;
        }

        public void CalcEstimate(int targetX, int targetY)
        {
            Estimate = Math.Abs(coordinates.x - targetX) + Math.Abs(coordinates.y - targetY);
        }

        public void CalculateValue(bool isDiagonal)
        {
            Value = (isDiagonal ? SecondStepCost : StepCost) + Estimate;
        }
        public override bool Equals(object? obj)
        {
            if (obj is not Point point)
            {
                return false;
            }

            return coordinates.x == point.coordinates.x && coordinates.y == point.coordinates.y;
        }
    }
}
