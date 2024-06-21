using Model;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Pathfinding;
using UnityEngine;

namespace Assets.Scripts.UnitBrains.Pathfinding
{
    public partial class AStarPath : BaseUnitPath
    {
        private int _maxLength = 100;

        private readonly Vector2Int[] _directions = {
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.up,
            Vector2Int.right,
        };

        public AStarPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint)
            : base(runtimeModel, startPoint, endPoint)
        {
            _maxLength = runtimeModel.RoMap.Width * runtimeModel.RoMap.Height;
        }

        protected override void Calculate()
        {
            var route = CalculateAStar(startPoint, endPoint);
            if (route != null)
            {
                var r = getPathFromNode(route);
                r.Reverse();
                path = r.ToArray();
            }
        }

        private List<Vector2Int> getPathFromNode(AStarPathNode node)
        {
            var _path = new List<Vector2Int>();

            while (node != null)
            {
                _path.Add(node.ToVector2());
                node = node.Parent;
            }

            return _path;
        }

        private AStarPathNode getBestNode(List<AStarPathNode> openlist)
        {
            if (openlist.Count == 0)
            {
                return null;
            }

            int bestIndex = 0;

            for (int i = 0; i < openlist.Count; i++)
            {
                if (openlist[i].Value < openlist[bestIndex].Value)
                {
                    bestIndex = i;
                }
            }

            AStarPathNode bestNode = openlist[bestIndex];
            openlist.RemoveAt(bestIndex);

            return bestNode;
        }

        private AStarPathNode CalculateAStar(Vector2Int fromPos, Vector2Int toPos)
        {
            var startNode = new AStarPathNode(fromPos);
            startNode.CalculateEstimate(toPos.x, toPos.y);
            var openList = new List<AStarPathNode> { startNode };
            var closedList = new HashSet<Vector2Int>();
            bool routeFound = false;
            var counter = 0;
            AStarPathNode currentNode = null;

            while (openList.Count > 0 && counter++ < _maxLength)
            {
                currentNode = getBestNode(openList);
                if (currentNode == null)
                {
                    break;
                }

                if (routeFound)
                {
                    return currentNode;
                }

                closedList.Add(currentNode.ToVector2());

                for (int i = 0; i < _directions.Length; i++)
                {
                    Vector2Int direction = _directions[i];
                    Vector2Int neighborPoint = currentNode.ToVector2() + direction;

                    if (closedList.Contains(neighborPoint))
                    {
                        continue;
                    }

                    if (neighborPoint == endPoint)
                    {
                        routeFound = true;
                    }

                    if (IsTileValid(neighborPoint) || routeFound)
                    {
                        CalculateNeigborWeights(openList, currentNode, neighborPoint, endPoint);
                    }
                }

            }
            return currentNode;
        }

        private bool IsTileValid(Vector2Int neighborPoint) =>
            neighborPoint.x >= 0
            && neighborPoint.x < runtimeModel.RoMap.Width
            && neighborPoint.y >= 0
            && neighborPoint.y < runtimeModel.RoMap.Height
            && runtimeModel.IsTileWalkable(neighborPoint);

        private void CalculateNeigborWeights(List<AStarPathNode> openList, AStarPathNode currentNode, Vector2Int neighborPoint, Vector2Int endPoint)
        {
            if (!openList.Any(n => n.ToVector2() == neighborPoint))
            {
                var newNode = new AStarPathNode(neighborPoint);
                newNode.CalculateEstimate(endPoint.x, endPoint.y);
                newNode.Parent = currentNode;

                openList.Add(newNode);
            }
        }
    }
}
