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
        private bool _isEnemyEncountered;
        private Node _nextToBotUnit;
        
        
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
                
                // Если смогли достичь базы - строим до неё путь.
                if (_isTargetReached)
                {
                    path = BuildPath(currentNode);
                    return;
                }
                
                CheckNeighborTiles(currentNode, targetNode, openList, closedList);
            }

            // Если не смогли достичь базы - строим путь до первого встречного врага.
            if (_isEnemyEncountered)
            {
                path = BuildPath(_nextToBotUnit);
                return;
            }
            
            // Если все пути движения заблокированы другими юнитами - возвращаем текущую позицию юнита.
            path = new []{startNode.Pos};
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
                
                if (CheckEncounterWithEnemy(newTilePos) && !_isEnemyEncountered)
                {
                    _isEnemyEncountered = true;
                    _nextToBotUnit = currentNode;
                }
            }
        }
        
        
        private bool CheckEncounterWithEnemy(Vector2Int newPos)
        {
            var botUnitPositions = runtimeModel.RoBotUnits.Select(u => u.Pos)
                                                                         .Where(u => u == newPos);

            return botUnitPositions.Any();
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