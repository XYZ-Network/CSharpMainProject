using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    public class AStarUnitPath : BaseUnitPath
    {
        private class Node
        {
            public int X;
            public int Y;
            public int Cost;
            public int Estimate;
            public int Value;
            public Node Parent;

            public Node(int x, int y)
            {
                X = x;
                Y = y;
                Cost = 0;
            }

            public void CalculateEstimate(int targetX, int targetY)
            {
                int distanceX = Math.Abs(X - targetX);
                int distanceY = Math.Abs(Y - targetY);
                Estimate = distanceX + distanceY;
            }

            public void CalculateValue()
            {
                Value = Cost + Estimate;
            }
        }

        private readonly int[] dx = { -1, 0, 1, 0 };
        private readonly int[] dy = { 0, 1, 0, -1 };

        public AStarUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint)
            : base(runtimeModel, startPoint, endPoint)
        {
        }

        protected override void Calculate()
        {

            if (startPoint == endPoint)
            {
                path = new[] { startPoint };
                return;
            }

            Node startNode = new Node(startPoint.x, startPoint.y);
            Node targetNode = new Node(endPoint.x, endPoint.y);

            List<Node> openList = new List<Node>();
            List<Node> closedList = new List<Node>();

            startNode.CalculateEstimate(targetNode.X, targetNode.Y);
            startNode.CalculateValue();
            openList.Add(startNode);


            while (openList.Count > 0)
            {

                Node currentNode = openList[0];
                int currentIndex = 0;


                for (int i = 1; i < openList.Count; i++)
                {
                    if (openList[i].Value < currentNode.Value ||
                        (openList[i].Value == currentNode.Value && openList[i].Estimate < currentNode.Estimate))
                    {
                        currentNode = openList[i];
                        currentIndex = i;
                    }
                }


                openList.RemoveAt(currentIndex);


                if (currentNode.X == targetNode.X && currentNode.Y == targetNode.Y)
                {
                    ReconstructPath(currentNode);
                    return;
                }


                closedList.Add(currentNode);


                for (int i = 0; i < 4; i++)
                {
                    int newX = currentNode.X + dx[i];
                    int newY = currentNode.Y + dy[i];


                    if (!IsWalkable(newX, newY))
                        continue;


                    bool inClosedList = false;
                    foreach (var node in closedList)
                    {
                        if (node.X == newX && node.Y == newY)
                        {
                            inClosedList = true;
                            break;
                        }
                    }
                    if (inClosedList)
                        continue;


                    Node neighbor = new Node(newX, newY);
                    neighbor.Cost = currentNode.Cost + 1;
                    neighbor.Parent = currentNode;
                    neighbor.CalculateEstimate(targetNode.X, targetNode.Y);
                    neighbor.CalculateValue();


                    bool inOpenList = false;
                    for (int j = 0; j < openList.Count; j++)
                    {
                        if (openList[j].X == newX && openList[j].Y == newY)
                        {
                            inOpenList = true;

                            if (neighbor.Value < openList[j].Value)
                            {
                                openList[j].Cost = neighbor.Cost;
                                openList[j].Parent = currentNode;
                                openList[j].CalculateEstimate(targetNode.X, targetNode.Y);
                                openList[j].CalculateValue();
                            }
                            break;
                        }
                    }


                    if (!inOpenList)
                    {
                        openList.Add(neighbor);
                    }
                }
            }


            path = new Vector2Int[0];
        }

        private void ReconstructPath(Node endNode)
        {
            List<Vector2Int> pathList = new List<Vector2Int>();
            Node current = endNode;

            while (current != null)
            {
                pathList.Add(new Vector2Int(current.X, current.Y));
                current = current.Parent;
            }

            pathList.Reverse();
            path = pathList.ToArray();
        }

        private bool IsWalkable(int x, int y)
        {
            Vector2Int pos = new Vector2Int(x, y);


            if (x < 0 || x >= runtimeModel.RoMap.Width ||
                y < 0 || y >= runtimeModel.RoMap.Height)
            {
                return false;
            }


            if (pos == endPoint)
            {
                return true;
            }


            if (runtimeModel.RoMap[pos])
            {
                return false;
            }

            return true;
        }
    }
}