using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnitBrains.Pathfinding
{

    public class Node
    {
        public Vector2Int Position { get; }
        public int Cost { get; } = 10;
        public int Estimate { get; private set; }
        public int Value { get; private set; }
        public Node Parent { get; set; }

        public Node(Vector2Int position)
        {
            Position = position;
        }
        public Node(Vector2Int position, int cost)
        {
            Position = position;
            Cost = cost;
        }

        public void CalculateEstimate(Vector2Int target)
        {
            var differentPosition = target - Position;
            Estimate = (int)differentPosition.magnitude;
        }

        public void CalculateValue() { Value = Cost + Estimate; }
        public override bool Equals(object obj)
        {
            return obj is Node node && Position.Equals(node.Position);
        }
        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }
    }
}