using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitBrains.Pathfinding;
using UnityEngine;
using Model.Runtime.ReadOnly;

namespace UnitBrains.Pathfinding
{
    public class NewUnitPath : BaseUnitPath
    {
        private class Node
        {
            public int X;
            public int Y;
            public int G;
            public int H;
            public int Value => G + H;
            public Node Parent;

            public Node(int x, int y)
            {
                X = x;
                Y = y;
            }

            public void CalculateEstimate(int targetX, int targetY)
            {
                H = Math.Abs(X - targetX) + Math.Abs(Y - targetY);
            }

            public override bool Equals(object obj)
            {
                if (obj is not Node node)
                    return false;

                return X == node.X && Y == node.Y;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(X, Y);
            }

            public Vector2Int ToVector() => new(X, Y);
        }

        private readonly int[] dx = { -1, 0, 1, 0 };
        private readonly int[] dy = { 0, 1, 0, -1 };

        private Vector2Int _currentPosition;
        private bool _positionChanged = false;

        public NewUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint) : base(runtimeModel, startPoint, endPoint)
        {
            _currentPosition = startPoint;
        }

        public override Vector2Int GetNextStepFrom(Vector2Int currentPos)
        {
            if (currentPos != _currentPosition)
            {
                _currentPosition = currentPos;
                path = null;
            }
            if (path == null || path.Length == 0)
            {
                Calculate();
            }

            if (path == null || path.Length == 0)
            {
                return _currentPosition;
            }

            int currentIndex = Array.IndexOf(path, _currentPosition);
            if (currentIndex < 0)
            {
                path = null;
                Calculate();
                currentIndex = Array.IndexOf(path, _currentPosition);
                if (currentIndex < 0)
                {
                    return _currentPosition;
                }
            }
            return currentIndex + 1 < path.Length ? path[currentIndex + 1] : _currentPosition;            
        }
        protected override void Calculate()
        {
            path = null;

            if (_currentPosition == endPoint)
            {
                path = new[] { _currentPosition };
                return;
            }
            if (!_positionChanged && path != null)
                return;

            _positionChanged = false;

            if (_currentPosition == endPoint)
            {
                path = new[] { _currentPosition };
                return;
            }

            Node startNode = new Node(_currentPosition.x, _currentPosition.y) { G = 0 };
            Node targetNode = new Node(endPoint.x, endPoint.y);
            startNode.CalculateEstimate(targetNode.X, targetNode.Y);

            List<Node> openList = new() { startNode };
            HashSet<Node> closedList = new();
            Dictionary<Vector2Int, Node> allNodes = new() { [_currentPosition] = startNode };

            Node bestNode = startNode;

            while (openList.Count > 0)
            {
                Node currentNode = openList[0];
                for (int i = 1; i < openList.Count; i++)
                {
                    if (openList[i].Value < currentNode.Value ||
                        openList[i].Value == currentNode.Value && openList[i].H < currentNode.H)
                    {
                        currentNode = openList[i];
                    }
                }

                if (currentNode.H < bestNode.H)
                    bestNode = currentNode;

                if (currentNode.X == targetNode.X && currentNode.Y == targetNode.Y)
                {
                    ReconstructPath(currentNode);
                    return;
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                for (int i = 0; i < dx.Length; i++)
                {
                    Vector2Int neighborPos = new(currentNode.X + dx[i], currentNode.Y + dy[i]);

                    if (!IsValid(neighborPos))
                        continue;

                    if (!allNodes.TryGetValue(neighborPos, out Node neighbor))
                    {
                        neighbor = new Node(neighborPos.x, neighborPos.y);
                        neighbor.CalculateEstimate(targetNode.X, targetNode.Y);
                        allNodes[neighborPos] = neighbor;
                    }

                    if (closedList.Contains(neighbor))
                        continue;

                    int tentativeG = currentNode.G + 10;

                    if (tentativeG < neighbor.G || !openList.Contains(neighbor))
                    {
                        neighbor.G = tentativeG;
                        neighbor.Parent = currentNode;

                        if (!openList.Contains(neighbor))
                            openList.Add(neighbor);
                    }
                }
            }
            ReconstructPath(bestNode);
        }

        private void ReconstructPath(Node endNode)
        {
            List<Vector2Int> pathList = new();
            Node current = endNode;

            while (current != null)
            {
                pathList.Add(current.ToVector());
                current = current.Parent;
            }

            pathList.Reverse();
            path = pathList.ToArray();
        }

        private bool IsValid(Vector2Int pos)
        {
            if (pos.x < 0 || pos.x >= runtimeModel.RoMap.Width ||
                pos.y < 0 || pos.y >= runtimeModel.RoMap.Height)
                    return false;

            if (runtimeModel.RoMap[pos])
                return false;

            if (pos == _currentPosition)
                return true;

            if (pos == endPoint)
                return true;

            foreach (var unit in runtimeModel.RoUnits)
            {
                if (unit.Pos == pos)
                    return false;
            }
            return true;
        }
    }
}
