using Model;
using Model.Runtime;
using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace UnitBrains.Pathfinding
{
    public class AstarUnitPath : BaseUnitPath
    {

        private Vector2Int[] dx = { 
            Vector2Int.down, 
            Vector2Int.up, 
            Vector2Int.left, 
            Vector2Int.right
        };
        private const int MaxLength = 100;

        public AstarUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint) : base(runtimeModel, startPoint, endPoint)
        {
        }

        protected override void Calculate()
        {
            var listPath = FindPath();
            if (listPath == null)
            {
                path = new Vector2Int[]{ };
                return;
            }
            path = listPath.ToArray();
        }

        private List<Vector2Int> FindPath()
        {
            Node startNode = new Node(startPoint);
            Node targetNode = new Node(endPoint);

            List<Node> openNode = new List<Node>() { startNode };
            HashSet<Node> closedList = new HashSet<Node>() { };
            int counter = 0;
            Node currentNode = startNode;
            while (openNode.Count > 0 && MaxLength > counter)
            {
                currentNode = openNode[0];
                foreach (Node node in openNode)
                {
                    if (node.Value < currentNode.Value)
                        currentNode = node;
                }

                openNode.Remove(currentNode);
                closedList.Add(currentNode);
                if (endPoint.Equals(currentNode.Pos))
                {
                    return buildPath(currentNode);
                }

                foreach (var target in dx)
                {
                    var nextStep = target + currentNode.Pos;
                    if (runtimeModel.IsTileWalkable(nextStep) || nextStep.Equals(endPoint))
                    {
                        Node neighbor = new Node(nextStep);
                        if (closedList.Contains(neighbor))
                            continue;

                        neighbor.Parent = currentNode;
                        neighbor.CalculateEstimate(targetNode.Pos);
                        neighbor.CalculateValue();
                        openNode.Add(neighbor);
                    }
                }
                counter++;
            }

            return buildPath(currentNode);
        }

        private List<Vector2Int> buildPath(Node node)
        {
            List<Vector2Int> result = new();
            while (node != null)
            {
                result.Add(node.Pos);
                node = node.Parent;
            }
            result.Reverse();
            return result;
        }

        public class Node
        {
            public Vector2Int Pos;
            public int Cost = 10;
            public int Estimate;
            public int Value;
            public Node Parent;

            public Node(Vector2Int pos)
            {
                Pos = pos;
            }

            public void CalculateEstimate(Vector2Int target)
            {
                var diff = target - Pos;
                Estimate = Math.Abs(diff.x) + Math.Abs(diff.y);
            }

            public void CalculateValue()
            {
                Value = Cost + Estimate;
            }

            public override bool Equals(object obj)
            {
                return obj is Node node &&
                       Pos.x.Equals(node.Pos.x) &&
                       Pos.y.Equals(node.Pos.y);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Pos.x, Pos.y);
            }
        }

    }
}