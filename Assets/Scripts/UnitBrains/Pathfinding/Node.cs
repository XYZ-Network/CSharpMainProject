using UnityEngine;

namespace UnitBrains.Pathfinding
{
    public class Node {
        public readonly Vector2Int Pos;
        public int Cost; // cost from start pos
        public int Estimate; // estimate cost to end pos
        public int Value => Cost + Estimate;
        public Node Parent;

        public Node(Vector2Int coord, int estimate = 0, int cost = 0, Node parent = null) {
            Pos = coord;
            Cost = cost;
            Estimate = estimate;
            Parent = parent;
        }
    }
}