using System;
using System.Collections.Generic;
using Model;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    public partial class A_StartUnitPath : BaseUnitPath
    {
        private int[] _dx = { -1, 0, 1, 0 };
        private int[] _dy = { 0, 1, 0, -1 };

        public A_StartUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint) 
            : base(runtimeModel, startPoint, endPoint)
        {
        }

        private bool IsValid(int x, int y)
        {
            bool containsX = x >= 0 && x < runtimeModel.RoMap.Width;
            bool containsY = y >= 0 && y < runtimeModel.RoMap.Height;
            return containsX && containsY && (runtimeModel.IsTileWalkable(new Vector2Int(x, y)) || (x == endPoint.x && y == endPoint.y));
        }

        protected override void Calculate()
        {
            path = null;
            Node startNode = new Node(startPoint);
            Node targetNode = new Node(endPoint);

            List<Node> openList = new List<Node>() { startNode };
            List<Node> closedList = new List<Node>();

            Node currentNode = null;
            while (openList.Count < runtimeModel.RoMap.Width * runtimeModel.RoMap.Height)
            {
                if (openList.Count > 0)
                    currentNode = openList[0];
                else
                {
                    path = new Vector2Int[] { new Vector2Int(startPoint.x, startPoint.y) };
                    return;
                }

                foreach (var node in openList)
                {
                    if (node.Value < currentNode.Value)
                        currentNode = node;
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                for (int i = 0; i < _dx.Length; i++)
                {
                    int newX = currentNode.coordinates.x + _dx[i];
                    int newY = currentNode.coordinates.y + _dy[i];

                    if (newX == targetNode.coordinates.x && newY == targetNode.coordinates.y)
                    {
                        path = ReverseResult(currentNode);
                        return;
                    }

                    if (IsValid(newX, newY))
                    {
                        Node neighbor = new Node(new Vector2Int(newX, newY));

                        if (closedList.Contains(neighbor))
                            continue;

                        neighbor.Parent = currentNode;
                        neighbor.CalculateEstimate(targetNode.coordinates.x, targetNode.coordinates.y);
                        neighbor.CalculateValue((i + 1) % 2 == 0);

                        openList.Add(neighbor);
                    }
                }
            }

            path = ReverseResult(currentNode);
            if (path.Length == 0)
                throw new Exception("Temporarily stuck");
        }

        private Vector2Int[] ReverseResult(Node currentNode)
        {
            List<Node> path = new List<Node>();

            while (currentNode != null)
            {
                path.Add(currentNode);
                currentNode = currentNode.Parent;
            }

            path.Reverse();
            List<Vector2Int> tempResult = new List<Vector2Int>();
            foreach (var node in path)
            {
                tempResult.Add(new Vector2Int(node.coordinates.x, node.coordinates.y));
            }

            return tempResult.ToArray();
        }
    }
}