using Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace UnitBrains.Pathfinding
{
    public class AStarUnitPath : BaseUnitPath
    {
        private Vector2Int _startPoint;
        private Vector2Int _endPoint;
        private int[] dx = { -1, 0, 1, 0 };
        private int[] dy = { 0, -1, 0, 1 };
        public AStarUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint) : base(runtimeModel, startPoint, endPoint)
        {
            _startPoint = startPoint;
            _endPoint = endPoint;
        }

        protected override void Calculate()
        {
            if (FindPath() is not null)
            {
                path = FindPath().ToArray();
            }
            else
            {
                path = null;
            }

            if (path == null)
                path = new Vector2Int[] { StartPoint };
        }


        public List<Vector2Int> FindPath()
        {
            Node startNode = new Node(_startPoint);
            Node targetNode = new Node(_endPoint);
            List<Node> openList = new List<Node> { startNode };
            List<Node> closedList = new List<Node>();

            while (openList.Count > 0)
            {
                Node currentNode = openList[0];

                foreach (var node in openList)
                {
                    if (node.Value < currentNode.Value)
                        currentNode = node;
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                if (currentNode.Position.x == targetNode.Position.x && currentNode.Position.y == targetNode.Position.y)
                {
                    List<Vector2Int> path = new List<Vector2Int>();

                    while (currentNode != null)
                    {
                        path.Add(currentNode.Position);
                        currentNode = currentNode.Parent;
                    }

                    path.Reverse();
                    return path;
                }

                for (int i = 0; i < dx.Length; i++)
                {
                    Vector2Int neighborPos = new Vector2Int(currentNode.Position.x + dx[i], currentNode.Position.y + dy[i]);


                    if (!IsValid(neighborPos) && neighborPos != _endPoint && !IsBlockedByEnemy(neighborPos))
                        continue;


                    Node neighbor = new Node(neighborPos);

                    if (closedList.Contains(neighbor))
                        continue;

                    neighbor.Parent = currentNode;
                    neighbor.CalculateEstimate(targetNode.Position.x, targetNode.Position.y);
                    neighbor.CalculateValue();
                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                    }
                }
            }

            return null;
        }

        private bool IsValid(Vector2Int tempPos)
        {
            bool containsX = tempPos.x >= 0 && tempPos.x < runtimeModel.RoMap.Width;
            bool containsY = tempPos.y >= 0 && tempPos.y < runtimeModel.RoMap.Height;
            return containsX && containsY && runtimeModel.IsTileWalkable(tempPos);
        }

        private bool IsBlockedByEnemy(Vector2Int tempPos)
        {
            var botPos = runtimeModel.RoBotUnits.Select(u => u.Pos).Where(u => u == tempPos);
            return botPos.Any();
        }
    }
}
