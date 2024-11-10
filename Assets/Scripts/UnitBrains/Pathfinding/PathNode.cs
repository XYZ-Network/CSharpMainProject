using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnitBrains.Pathfinding
{

    public class PathNode
    {
        public Vector2Int Position { get; }
        public int Cost { get; } = 10;
        public int Estimate { get; private set; }
        public int Value { get; private set; }
        public PathNode Parent { get; set; }

        public PathNode(Vector2Int position)
        {
            Position = position;
        }
        public PathNode(Vector2Int position, int cost)
        {
            Position = position;
            Cost = cost;
        }

        public void CalculateEstimate(Vector2Int target)
        {
            var diffPos = target - Position;
            Estimate = (int)diffPos.magnitude;
        }

        public void CalculateValue() { Value = Cost + Estimate; }
        public override bool Equals(object obj)
        {
            return obj is PathNode node && Position.Equals(node.Position);
        }
        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }
    }
}
