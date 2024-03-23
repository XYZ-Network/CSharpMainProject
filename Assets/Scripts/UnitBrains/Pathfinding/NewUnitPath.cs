using JetBrains.Annotations;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using UnitBrains;
using UnitBrains.Pathfinding;
using UnityEngine;
using System.Net;
using System.Security.Cryptography;
using System.Drawing;
using Model.Runtime;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using Unity.VisualScripting;


namespace Assets.Scripts.UnitBrains.Pathfinding
{

    public partial class NewUnitPath : BaseUnitPath
    {
        private int[] dx = { -1, -1, 0, 1, 1, 1, 0, -1 };
        private int[] dy = { 0, 1, 1, 1, 0, -1, -1, -1 };

        public NewUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint) : base(runtimeModel, startPoint, endPoint)
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
            Point startPoint = new Point(StartPoint.x, StartPoint.y);
            Point targetPoint = new Point(endPoint.x, endPoint.y);

            List<Point> openList = new List<Point>() { startPoint };
            List<Point> closedList = new List<Point>();

            Point currentPoint = null;

            while (openList.Count < runtimeModel.RoMap.Width * runtimeModel.RoMap.Height)
            {
                if (openList.Count > 0)
                    currentPoint = openList[0];
                else
                {
                    path = new Vector2Int[]{ new Vector2Int(StartPoint.x, StartPoint.y)};
                    return;
                }

                foreach (var point in openList)
                {
                    if (point.Value < currentPoint.Value)
                        currentPoint = point;
                }

                openList.Remove(currentPoint);
                closedList.Add(currentPoint);

                for (int i = 0; i < dx.Length; i++)
                {
                    int newX = currentPoint.coordinates.x + dx[i];
                    int newY = currentPoint.coordinates.y + dy[i];

                    if (newX == targetPoint.coordinates.x && newY == targetPoint.coordinates.y)
                    {
                        path = FullReverse(currentPoint);
                        return;
                    }

                    if (IsValid(newX, newY))
                    {
                        Point neighbor = new Point(newX, newY);

                        if (closedList.Contains(neighbor))
                            continue;

                        neighbor.parent = currentPoint;
                        neighbor.CalcEstimate(targetPoint.coordinates.x, targetPoint.coordinates.y);
                        neighbor.CalculateValue((i + 1) % 2 == 0);

                        openList.Add(neighbor);
                    }

                }
            }
            path = FullReverse(currentPoint);
            if (path.Length == 0)
                Debug.Log("Something went wrong");
        }
        private Vector2Int[] FullReverse(Point currentPoint)
        {
            List<Point> path = new List<Point>();

            while (currentPoint != null)
            {
                path.Add(currentPoint);
                currentPoint = currentPoint.parent;
            }

            path.Reverse();
            List<Vector2Int> tempResult = new List<Vector2Int>();
            foreach (var point in path)
            {
                tempResult.Add(new Vector2Int(point.coordinates.x, point.coordinates.y));
            }

            return tempResult.ToArray();
        }
    }
}



