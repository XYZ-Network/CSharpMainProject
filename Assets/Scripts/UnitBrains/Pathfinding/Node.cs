using System;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    public class Node
    {
        public Vector2Int Pos => _position;
        public int Value => _value;
        public Node Previous { get; set; }
        
        private readonly int _moveCost = 10;
        private Vector2Int _position;
        private int _estimateToTarget;
        private int _value;

        
        public Node(Vector2Int position)
        {
            _position = position;
        }
    
    
        public void CalculateEstimate(Vector2Int targetPos)
        {
            _estimateToTarget = Math.Abs(_position.x - targetPos.x) + Math.Abs(_position.y - targetPos.y);
        }
    
    
        public void CalculateValue()
        {
            _value = _moveCost + _estimateToTarget;
        }
    
    
        public override bool Equals(object? obj)
        {
            if (obj is not Node node)
                return false;
        
            return _position.x == node.Pos.x && _position.y == node.Pos.y;
        }
    }
}