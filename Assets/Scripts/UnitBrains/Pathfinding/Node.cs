using System;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    public class Node
    {
        public Vector2Int coordinates;
        public int Cost = 10;
        public int DiaginalCost = 14;
        public int Estimate;
        public int Value;
        public Node Parent;

        public Node(Vector2Int coordinates)
        {
            this.coordinates = coordinates;
        }

        public void CalculateEstimate(int targetX, int targetY)
        {
            Estimate = (Math.Abs(coordinates.x - targetX) + Math.Abs(coordinates.y - targetY)) * 10;
        }

        public void CalculateValue(bool isDiagonal)
        {
            Value = (isDiagonal ? DiaginalCost : Cost) + Estimate;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Node node)
            {
                return false;
            }

            return coordinates.x == node.coordinates.x && coordinates.y == node.coordinates.y;
        }
    }
}