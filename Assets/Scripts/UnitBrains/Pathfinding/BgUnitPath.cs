using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    public class BgUnitPath : BaseUnitPath
    {
        private int[] dx = { -1, 0, 1, 0 };
        private int[] dy = { 0, 1, 0, -1 };
        private Vector2Int _startPoint;
        private Vector2Int _endPoint;
        private bool _isTarget;
        private bool _isEnemyUnitClose;
        private Node _nextToEnemyUnit;

        public BgUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint)
            : base(runtimeModel, startPoint, endPoint)
        {
            _startPoint = startPoint;
            _endPoint = endPoint;
        }

        protected override void Calculate()
        {
            Node startNode = new Node(_startPoint);
            Node targetNode = new Node(_endPoint);
            List<Node> openList = new List<Node>() { startNode };
            List<Node> closedList = new List<Node>();

            int counter = 0;
            int maxCount = runtimeModel.RoMap.Width * runtimeModel.RoMap.Height;

            while (openList.Count > 0 && counter++ < maxCount)
            {
                Node currentNode = openList[0];

                foreach (var node in openList)
                {
                    if (node.Value < currentNode.Value)
                        currentNode = node;
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                if (_isTarget)
                {
                    path = FindPath(currentNode);
                    return;
                }

                for (int i = 0; i < dx.Length; i++)
                {
                    int newX = currentNode.Point.x + dx[i];
                    int newY = currentNode.Point.y + dy[i];
                    var newPoint = new Vector2Int(newX, newY);

                    if (newPoint == targetNode.Point)
                        _isTarget = true;

                    if (IsValid(newPoint) || _isTarget)
                    {
                        Node neighbor = new Node(newPoint);

                        if (closedList.Contains(neighbor))
                            continue;

                        neighbor.Parent = currentNode;
                        neighbor.CalculateEstimate(targetNode.Point.x, targetNode.Point.y);
                        neighbor.CalculateValue();
                        openList.Add(neighbor);
                    }
                    if (CheckCollisionWithEnemy(newPoint) && !_isEnemyUnitClose)
                    {
                        _isEnemyUnitClose = true;
                        _nextToEnemyUnit = currentNode;
                    }
                }
            }
            if (_isEnemyUnitClose)
            {
                path = FindPath(_nextToEnemyUnit);
                return;
            }

            path = new Vector2Int[] { startNode.Point };
        }

        private Vector2Int[] FindPath(Node node)
        {
            List<Vector2Int> path = new();

            while (node != null)
            {
                path.Add(node.Point);
                node = node.Parent;
            }

            path.Reverse();
            return path.ToArray();
        }

        private bool IsValid(Vector2Int point)
        {
            return runtimeModel.IsTileWalkable(point);
        }

        private bool CheckCollisionWithEnemy(Vector2Int newPos)
        {
            var botUnitPositions = runtimeModel.RoBotUnits.Select(u => u.Pos).Where(u => u == newPos);

            return botUnitPositions.Any();
        }
    }

    public class Node
    {
        public int Cost = 10;
        public int Estimate;
        public int Value;
        public Node Parent;
        public Vector2Int Point;

        public Node(Vector2Int point)
        {
            Point = point;
        }

        public void CalculateEstimate(int tarX, int tarY)
        {
            Estimate = Math.Abs(Point.x - tarX) + Math.Abs(Point.y - tarY);
        }

        public void CalculateValue()
        {
            Value = Cost + Estimate;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Node node)
                return false;
            return Point.x == node.Point.x && Point.y == node.Point.y;
        }
    }
}