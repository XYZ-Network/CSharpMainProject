using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    public class Node
    {
        public Vector2Int Pos;
        public int Cost = 1;
        public int Estimate;
        public int Value;
        public Node Parent;

        public Node(Vector2Int pos)
        {
            Pos = pos;
        }

        public override bool Equals(object obj)
        {
            return obj is Node n && Pos == n.Pos;
        }

        public override int GetHashCode()
        {
            return Pos.GetHashCode();
        }
    }
}

