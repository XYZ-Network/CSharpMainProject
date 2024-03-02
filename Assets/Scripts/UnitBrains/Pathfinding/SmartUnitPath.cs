using System.Collections.Generic;
using System.Linq;
using Model;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    public class SmartUnitPath : BaseUnitPath
    {
        private readonly int _maxPathLength = 100;
        private Vector2Int[] _directions;
        private bool _isTargetReached;
        
        
        public SmartUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint) 
                            : base(runtimeModel, startPoint, endPoint)
        {
            _directions = new[]
            {
                Vector2Int.left,
                Vector2Int.up,
                Vector2Int.right,
                Vector2Int.down,
            };
        }

        
        protected override void Calculate()
        {
            var startNode = new Node(startPoint);
            var targetNode = new Node(endPoint);
            
            var openList = new List<Node> {startNode};
            var closedList = new List<Node>();
            var nodesNumber = 0;
            
            while (openList.Any() && nodesNumber++ < _maxPathLength)
            {
                var currentNode = openList[0];

                foreach (var node in openList)
                {
                    if (node.Value < currentNode.Value)
                        currentNode = node;
                }
                
                openList.Remove(currentNode);
                closedList.Add(currentNode);
                
                
                if (_isTargetReached)
                {
                    path = BuildPath(currentNode);
                    return;
                }
                
                CheckNeighborTiles(currentNode, targetNode, openList, closedList);
            }
            
            path = closedList.Select(node => node.Pos).ToArray();
        }
        
        
        private void CheckNeighborTiles(Node currentNode, Node targetNode, List<Node> openList, List<Node> closedList)
        {
            foreach (var direction in _directions)
            {
                Vector2Int newTilePos = currentNode.Pos + direction;

                if (newTilePos == targetNode.Pos)
                    _isTargetReached = true;

                if (runtimeModel.IsTileWalkable(newTilePos) || _isTargetReached)
                {
                    Node newNode = new Node(newTilePos);
                    
                    if (closedList.Contains(newNode))
                        continue;
                    
                    newNode.Previous = currentNode;
                    newNode.CalculateEstimate(targetNode.Pos);
                    newNode.CalculateValue();
                    
                    openList.Add(newNode);
                }
            }
        }
        
        
        private Vector2Int[] BuildPath(Node currentNode)
        {
            List<Vector2Int> path = new();
        
            while (currentNode != null)
            {
                path.Add(currentNode.Pos);
                currentNode = currentNode.Previous;
            }
            
            path.Reverse();
            return path.ToArray();
        }
    }
}