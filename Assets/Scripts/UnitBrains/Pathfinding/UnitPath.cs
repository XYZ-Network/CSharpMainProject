using Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;
using UnitBrains;

namespace UnitBrains.Pathfinding
{
    public class UnitPath : BaseUnitPath
    {
        private int[] dx = { -1, 0, 1, 0 };
        private int[] dy = { 0, 1, 0, -1 };

        public UnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint) : base(runtimeModel, startPoint, endPoint)
        {
        }

        protected override void Calculate()
        {
            Debug.Log($"=== Calculate Path from {startPoint} to {endPoint} ===");

            List<Vector2Int> resultPath = FindPath();

            if (resultPath != null && resultPath.Count > 0)
            {
                path = resultPath.ToArray();
                Debug.Log($"SUCCESS: Path found with {path.Length} points");

                // ¬ыводим первые и последние несколько точек пути
                for (int i = 0; i < Mathf.Min(5, path.Length); i++)
                {
                    Debug.Log($"Path point {i}: {path[i]}");
                }
            }
            else
            {
                Debug.LogWarning($"FAILED: No path found from {startPoint} to {endPoint}");
                path = null;
            }
        }

        public List<Vector2Int> FindPath()
        {
            Node startNode = new Node(startPoint.x, startPoint.y);
            Node targetNode = new Node(endPoint.x, endPoint.y);

            List<Node> openList = new List<Node> { startNode };
            List<Node> closedList = new List<Node>();

            int iterations = 0;
            const int maxIterations = 10000;

            while (openList.Count > 0 && iterations < maxIterations)
            {
                iterations++;

                Node currentNode = openList.OrderBy(n => n.Value).First();

                if (Math.Abs(currentNode.X - targetNode.X) <= 1 && Math.Abs(currentNode.Y - targetNode.Y) <= 1)
                {
                    Debug.Log($"Path found in {iterations} iterations");
                    return ReconstructPath(currentNode);
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                for (int i = 0; i < dx.Length; i++)
                {
                    int newX = currentNode.X + dx[i];
                    int newY = currentNode.Y + dy[i];

                    if (!IsValid(newX, newY))
                        continue;

                    Node neighbor = new Node(newX, newY);

                    if (closedList.Contains(neighbor))
                        continue;

                    int tentativeG = currentNode.G + 1;

                    neighbor.CalculateEstimate(targetNode.X, targetNode.Y);

                    if (!openList.Contains(neighbor))
                    {
                        neighbor.Parent = currentNode;
                        neighbor.G = tentativeG;
                        neighbor.CalculateValue();
                        openList.Add(neighbor);
                    }
                    else
                    {
                        Node existingNode = openList.Find(n => n.Equals(neighbor));
                        if (tentativeG < existingNode.G)
                        {
                            existingNode.Parent = currentNode;
                            existingNode.G = tentativeG;
                            existingNode.CalculateValue();
                        }
                    }
                }
            }

            if (iterations >= maxIterations)
            {
                Debug.LogError($"A* exceeded max iterations ({maxIterations}) - path not found");
            }
            else
            {
                Debug.Log("Path not found - no route to destination");
            }

            return null;
        }

        private List<Vector2Int> ReconstructPath(Node endNode)
        {
            List<Vector2Int> path = new List<Vector2Int>();
            Node currentNode = endNode;

            while (currentNode != null)
            {
                path.Add(new Vector2Int(currentNode.X, currentNode.Y));
                currentNode = currentNode.Parent;
            }

            path.Reverse();
            return path;
        }

        private bool IsValid(int x, int y)
        {
            bool containsX = x >= 0 && x < runtimeModel.RoMap.Width;
            bool containsY = y >= 0 && y < runtimeModel.RoMap.Height;
            Vector2Int pos = new Vector2Int(x, y);
            bool isNotWall = runtimeModel.IsTileWalkable(pos);
            bool IsPlayerUnit = runtimeModel.RoPlayerUnits.Any(u => u.Pos == startPoint) ? true : false;
            if (IsPlayerUnit)
            {
                if (runtimeModel.RoBotUnits.Any(u => u.Pos == pos))
                {
                    return true;
                }
            }
            else
            {
                if (runtimeModel.RoPlayerUnits.Any(u => u.Pos == pos))
                {
                    return true;
                }
            }
            return containsX && containsY && isNotWall || (x == startPoint.x && y == startPoint.y);
        }
    }
}