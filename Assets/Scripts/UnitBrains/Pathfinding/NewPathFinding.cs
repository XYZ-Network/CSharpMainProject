using Codice.Client.Common.TreeGrouper;
using Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Pathfinding;
using UnityEngine;
using Utilities;

namespace UnitBrains.Pathfinding
{
    public class NewPathFinding : BaseUnitPath
    {
        private int[] dx = { -1, 0, 1, 0 };
        private int[] dy = { 0, 1, 0, -1 };
        private int MaxLength = 600;


        public NewPathFinding(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint)
            : base(runtimeModel, startPoint, endPoint)
        {
        }

        protected override void Calculate()
        {
            var points = FindPath();
            path = points == null ? new Vector2Int[] { StartPoint, StartPoint } : points.Select((point) => new Vector2Int(point.X, point.Y)).ToArray();
        }

        public List<Node> FindPath()
        {
            Node startPoint = new Node(this.startPoint.x, this.startPoint.y);
            Node endPoint = new Node(this.endPoint.x, this.endPoint.y);

            List<Node> openList = new List<Node>() { startPoint };
            List<Node> closedList = new List<Node>();


            while (openList.Count > 0)
            {

                Node currentPoint = openList[0];
                foreach (var point in openList)
                {
                    if (point.Value < currentPoint.Value)
                        currentPoint = point;
                }

                openList.Remove(currentPoint);
                closedList.Add(currentPoint);


                if (currentPoint.X == endPoint.X && currentPoint.Y == endPoint.Y || closedList.Count > MaxLength)
                {
                    List<Node> path = new List<Node>();
                    while (currentPoint != null)
                    {
                        path.Add(currentPoint);
                        currentPoint = currentPoint.Parent;
                    }
                    path.Reverse();
                    return path;
                }


                for (int i = 0; i < dx.Length; i++)
                {
                    int newX = currentPoint.X + dx[i];
                    int newY = currentPoint.Y + dy[i];

                    if (IsValid(new Vector2Int(newX, newY)))
                    {
                        Node nextPoint = new Node(newX, newY);


                        if (closedList.Contains(nextPoint))
                            continue;

                        nextPoint.Parent = currentPoint;
                        nextPoint.CalculateEstimate(endPoint.X, endPoint.Y);
                        nextPoint.CalculateValue();

                        openList.Add(nextPoint);
                    }
                }
            }

            return null;
        }

        private bool IsValid(Vector2Int point)
        {
            bool isValidX = point.y >= 0 && point.y < runtimeModel.RoMap.Height;
            bool isValidY = point.x >= 0 && point.x < runtimeModel.RoMap.Width;
            bool isBase = point.x == endPoint.x && point.y == endPoint.y;
            return isValidX && isValidY && (runtimeModel.IsTileWalkable(point) || isBase);
        }
    }
}