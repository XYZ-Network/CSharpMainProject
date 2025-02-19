using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Model;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    public class AStarUnitPath : BaseUnitPath
    {
        private int _maxCountLoop = 100;

        private BaseUnitBrain _baseUnitBrain;

        private List<Vector2Int> _diffVectors = new List<Vector2Int>()
        {
            new (1, -1),
            new (1, 1),
            new (1, 0),
            new (-1, -1),
            new (-1, 1),
            new (-1, 0),
            new (0, -1),
            new (0, 1)
        };

        public AStarUnitPath (IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint, BaseUnitBrain baseUnitBrain)
            : base (runtimeModel, startPoint, endPoint)
        {
            this._baseUnitBrain = baseUnitBrain;
        }

        protected override void Calculate()
        {
            AStarNode startNode = new AStarNode(startPoint);

            List<AStarNode> openList = new List<AStarNode>() { startNode };
            List<AStarNode> closedList = new List<AStarNode>();

            var counterLoopBreaker = 0;
            while (openList.Count > 0 && counterLoopBreaker++ < _maxCountLoop)
            {
                AStarNode currentNode = openList[0];

                foreach (var node in openList)
                {
                    if (node.Value < currentNode.Value)
                        currentNode = node;
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                if (_baseUnitBrain.IsTargetInRange(endPoint, currentNode.Pos))
                {
                    List<AStarNode> pathBuilding = new List<AStarNode>();

                    while (currentNode != null)
                    {
                        pathBuilding.Add(currentNode);
                        currentNode = currentNode.Parent;
                    }

                    pathBuilding.Reverse();
                    path = pathBuilding.Select(node => node.Pos).ToArray();
                    return;
                }

                foreach (var diffPos in _diffVectors)
                {
                    Vector2Int newPos = new();
                    newPos = currentNode.Pos + diffPos;

                    if (runtimeModel.IsTileWalkable(newPos))
                    {
                        AStarNode neighbor = new AStarNode(newPos);

                        if (closedList.Contains(neighbor))
                            continue;

                        neighbor.Parent = currentNode;
                        neighbor.CalculateEstimate(endPoint);
                        neighbor.CalculateValue();

                        openList.Add(neighbor);
                    }
                }
            }

            var defaultPath = new List<Vector2Int> { startPoint, startPoint };
            path = defaultPath.ToArray();
        }
    }
}
