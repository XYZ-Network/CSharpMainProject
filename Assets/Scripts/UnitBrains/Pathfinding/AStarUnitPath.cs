using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    public class AStarUnitPath : BaseUnitPath
    {
        private int MaxLength = 100;
        private Vector2Int[] successors = {
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.up,
            Vector2Int.right,
        };
        public AStarUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint) : base(runtimeModel, startPoint, endPoint)
        {
            MaxLength = runtimeModel.RoMap.Width * runtimeModel.RoMap.Height;
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

        private int CalculateEstimate(Vector2Int fromPos, Vector2Int toPos) => Math.Abs(fromPos.x - toPos.x) + Math.Abs(fromPos.y - toPos.y);

        private List<Vector2Int> getPathFromNode(Node node)
        {
            List<Vector2Int> _path = new();
            while (node != null)
            {
                _path.Add(node.Pos);
                node = node.Parent;
            }
            return _path;
        }

        private Node getBestNode(List<Node> frontier)
        {
            if (frontier.Count == 0)
            {
                return null;
            }
            int bestIndex = 0;

            for (int i = 0; i < frontier.Count; i++)
            {
                if (frontier[i].Value < frontier[bestIndex].Value)
                {
                    bestIndex = i;
                }
            }
            Node bestNode = frontier[bestIndex];
            frontier.RemoveAt(bestIndex);
            return bestNode;
        }

        private Node CalculateAStar(Vector2Int fromPos, Vector2Int toPos)
        {
            Node startNode = new(fromPos, CalculateEstimate(fromPos, toPos));
            List<Node> frontier = new() { startNode };
            HashSet<Vector2Int> visited = new();
            bool routeFound = false;
            var counter = 0;
            Node currentNode = null;

            while (frontier.Count > 0 && counter++ < MaxLength)
            {
                currentNode = getBestNode(frontier);
                if (currentNode == null)
                {
                    break;
                }

                if (routeFound)
                {
                    return currentNode;
                }

                visited.Add(currentNode.Pos);

                for (int i = 0; i < successors.Length; i++)
                {
                    Vector2Int s = successors[i];
                    Vector2Int neighborPoint = currentNode.Pos + s;

                    if (visited.Contains(neighborPoint))
                        continue;

                    if (neighborPoint == endPoint)
                    {
                        routeFound = true;
                    }
                    if (IsTileValid(neighborPoint) || routeFound)
                        CalculateNeigborWeights(frontier, currentNode, neighborPoint, endPoint);
                }

            }
            return currentNode;
        }

        private bool IsTileValid(Vector2Int neighborPoint) => 
            neighborPoint.x < 0 || neighborPoint.x >= runtimeModel.RoMap.Width || neighborPoint.y < 0 || neighborPoint.y >= runtimeModel.RoMap.Height
                ? false
                : runtimeModel.IsTileWalkable(neighborPoint);

        private void CalculateNeigborWeights(List<Node> openList, Node currentNode, Vector2Int neighborPoint,  Vector2Int endPoint)
        {
            if (!openList.Any(n => n.Pos == neighborPoint))
            {
                openList.Add(new Node(
                    neighborPoint,
                    CalculateEstimate(neighborPoint, endPoint),
                    1,
                    currentNode));
            }
        }
    }
}