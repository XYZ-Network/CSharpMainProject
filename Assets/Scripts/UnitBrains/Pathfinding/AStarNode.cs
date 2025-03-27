using System;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    public class AStarNode
    {
        public Vector2Int Pos { get; private set; }

        private int Cost = 10;
        private int Estimate;
        public int Value { get; private set; } = 0;

        public AStarNode Parent;

        public AStarNode(Vector2Int pos)
        {
            Pos = pos;
        }

        public void CalculateEstimate(Vector2Int targetPos)
        {
            Estimate = Math.Abs(Pos.x - targetPos.x) + Math.Abs(Pos.y - targetPos.y);
        }

        public void CalculateValue()
        {
            Value = Cost + Estimate;
        }

        public override bool Equals(object obj)
        {
            if (obj is not AStarNode node)
                return false;

            return Pos == node.Pos;
        }
    }
}
