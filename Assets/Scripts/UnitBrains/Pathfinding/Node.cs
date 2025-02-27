using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    public class Node
    {
        public Vector2Int position;
        public int Cost = 10;
        public int Estimate;
        public int Value;
        public Node Parent;

        public Node(Vector2Int position)
        {
            this.position = position;
        }
        public void CalculateEstimate(Vector2Int targetPos)
        {
            Estimate = Math.Abs(position.x - targetPos.x) + Math.Abs(position.y - targetPos.y);
        }
        public void CalculateValue()
        {
            Value = Cost + Estimate;
        }
    }
}
