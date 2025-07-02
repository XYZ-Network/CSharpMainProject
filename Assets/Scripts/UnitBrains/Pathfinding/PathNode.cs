using System;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    internal partial class AStarUnitPath
    {
        private class PathNode
        {
            public Vector2Int Position { get; }
            public int CostFromStart { get; set; }
            public int Estimate { get; private set; }
            public int Value { get; private set; }
            public PathNode Parent { get; set; }

            public PathNode(Vector2Int position, int costFromStart)
            {
                Position = position;
                CostFromStart = costFromStart;
            }

            public void CalculateEstimate(Vector2Int targetPos)
            {
                int dx = Mathf.Abs(targetPos.x - Position.x);
                int dy = Mathf.Abs(targetPos.y - Position.y);
                Estimate = 10 * Mathf.Max(dx, dy);
            }

            public void CalculateValue()
            {
                Value = CostFromStart + Estimate;
            }

            public override bool Equals(object obj)
            {
                if (obj is PathNode other)
                    return Position.Equals(other.Position);
                return false;
            }

            public override int GetHashCode()
            {
                return Position.GetHashCode();
            }
        }
    }
}