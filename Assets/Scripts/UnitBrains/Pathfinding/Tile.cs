using System;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    public class Tile
    {
        public Vector2Int Pos;
        public int Cost = 10;
        public int Estimate;
        public int Value;
        public Tile Parent;

        public Tile(Vector2Int pos)
        {
            Pos = pos;
        }

        public void CalculateEstimate(Vector2Int Target)
        {
            var diff = Target - Pos;
            Estimate = Math.Abs(diff.x) + Math.Abs(diff.y);
        }

        public void CalculateValue()
        {
            Value = Cost + Estimate;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is Tile other) return Pos.Equals(other.Pos);

            return false;
        }

        public override int GetHashCode() => Pos.GetHashCode();

    }
}

