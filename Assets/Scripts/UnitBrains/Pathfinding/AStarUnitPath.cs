using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;

namespace UnitBrains.Pathfinding
{
    public class AStarUnitPath : BaseUnitPath
    {
        private class Node
        {
            public Vector2Int Position;
            public Node Parent;
            public int G;
            public int H;
            public int F => G + H;

            public Node(Vector2Int position, Node parent, int g, int h)
            {
                Position = position;
                Parent = parent;
                G = g;
                H = h;
            }

            public override bool Equals(object obj)
            {
                return obj is Node node && node.Position == Position;
            }

            public override int GetHashCode() => Position.GetHashCode();
        }

        private static readonly Vector2Int[] Directions =
        {
            new(1, 0), 
            new(-1, 0),
            new(0, 1), 
            new(0, -1)
        };

        private Vector2Int _currentPosition;

        public AStarUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint) : base(runtimeModel, startPoint, endPoint)
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

            if (path == null || path.Length == 0) Calculate();

            if (path == null || path.Length == 0) return _currentPosition;

            int currentIndex = Array.IndexOf(path, _currentPosition);
            // позиция юнита вне path
            if (currentIndex < 0) 
            {
                Calculate();
                currentIndex = Array.IndexOf(path, _currentPosition);
                if (currentIndex < 0) return _currentPosition;
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

            var openSet = new List<Node>();
            var closedSet = new HashSet<Node>();

            Node startNode = new Node(_currentPosition, null, 0, Heuristic(_currentPosition, endPoint));
            Node targetNode = new Node(endPoint, null, 0, 0);

            openSet.Add(startNode);

            Node bestNode = startNode;

            while (openSet.Count > 0)
            {
                // выбираем ноду с минимальным F
                Node current = openSet[0];
                foreach (var node in openSet)
                {
                    if (node.F < current.F || (node.F == current.F && node.H < current.H))
                        current = node;
                }

                if (current.H < bestNode.H)
                    bestNode = current;

                if (current.Position == targetNode.Position)
                {
                    ReconstructPath(current);
                    return;
                }

                openSet.Remove(current);
                closedSet.Add(current);

                foreach (var dir in Directions)
                {
                    var neighborPos = current.Position + dir;
                    if (!IsValid(neighborPos))
                        continue;

                    var neighbor = new Node(neighborPos, current, current.G + 10, Heuristic(neighborPos, endPoint));

                    if (closedSet.Contains(neighbor))
                        continue;

                    var existing = openSet.FirstOrDefault(n => n.Position == neighborPos);
                    if (existing == null)
                    {
                        openSet.Add(neighbor);
                    }
                    else if (neighbor.G < existing.G)
                    {
                        existing.G = neighbor.G;
                        existing.Parent = current;
                    }
                }
            }

            ReconstructPath(bestNode);
        }

        private void ReconstructPath(Node endNode)
        {
            var result = new List<Vector2Int>();
            Node cur = endNode;
            while (cur != null)
            {
                result.Add(cur.Position);
                cur = cur.Parent;
            }
            result.Reverse();
            path = result.ToArray();
        }

        private bool IsValid(Vector2Int pos)
        {
            if (pos.x < 0 || pos.x >= runtimeModel.RoMap.Width ||
                pos.y < 0 || pos.y >= runtimeModel.RoMap.Height)
                return false;

            if (runtimeModel.RoMap[pos])
                return false;

            if (pos == _currentPosition || pos == endPoint)
                return true;

            foreach (var unit in runtimeModel.RoUnits)
            {
                if (unit.Pos == pos)
                    return false;
            }
            return true;
        }

        private int Heuristic(Vector2Int a, Vector2Int b)
        {
            return (Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y)) * 10;
        }
    }
}
