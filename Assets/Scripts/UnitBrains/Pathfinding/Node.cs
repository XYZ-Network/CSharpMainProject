using System;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    public class Node
    {
        public Vector2Int Pos;
        public int BaseCost = 10;
        public int Estimate;
        public int Value;
        public Node Parent;

        public Node(Vector2Int position)
        {
            Pos = position;
        }

        public void CalculateValue()
        {
            Value = BaseCost + Estimate;
        }

        public void CalculateEstimate(Vector2Int targetPos)
        {
            Estimate = Math.Abs(Pos.x - targetPos.x) + Math.Abs(Pos.y - targetPos.y);
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Node node)
                return false;

            return Pos.x == node.Pos.x && Pos.y == node.Pos.y;
        }
    }
}