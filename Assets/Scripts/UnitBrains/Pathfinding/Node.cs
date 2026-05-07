using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    public class Node
    {
        public Vector2Int Pos;
        public int Cost = 10;
        public int Estimate;
        public int Value;
        public Node Parent;

        public Node(Vector2Int pos)
        {
            Pos = pos;
        }

        public void CalculateEstimate(Vector2Int target)
        {
            var diff = target - Pos;
            Estimate = Math.Abs(diff.x) + Math.Abs(diff.y);
        }

        public void CalculateValue()
        {
            Value = Cost + Estimate;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is Node other)
                return Pos.Equals(other.Pos);
            return false;
        }

        public override int GetHashCode() => Pos.GetHashCode();
    }
}