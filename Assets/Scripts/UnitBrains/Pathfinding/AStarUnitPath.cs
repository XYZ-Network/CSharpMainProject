using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model;
using UnitBrains.Pathfinding;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

namespace Assets.Scripts.UnitBrains.Pathfinding
{
    public class AStarUnitPath : BaseUnitPath
    {
        private int[] dx = { -1, 0, 1, 0 };
        private int[] dy = { 0, 1, 0, -1 };

        public AStarUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint) : base(runtimeModel, startPoint, endPoint)
        {

        }

        protected override void Calculate()
        {
            Node startNode = new Node(this.startPoint);
            Node targetNode = new Node(this.endPoint);

            List<Node> openList = new List<Node> { startNode };
            List<Node> closedList = new List<Node>();

            while (openList.Count > 0)
            {

                Node currentNode = openList[0];
                foreach (var node in openList)
                {
                    if (node.Value < currentNode.Value)
                    {
                        currentNode = node;
                    }
                }

                openList.Remove(currentNode);
                if (!closedList.Contains(currentNode))
                {
                    closedList.Add(currentNode);
                }

                if (currentNode.Position == targetNode.Position)
                {
                    List<Node> newPath = new List<Node>();

                    while (currentNode != null)
                    {
                        newPath.Add(currentNode);
                        currentNode = currentNode.Parent;
                    }

                    newPath.Reverse();
                    path = newPath.Select(node => node.Position).ToArray();
                    return;
                }

                for (int i = 0; i < dx.Length; i++)
                {
                    Vector2Int currentPosition = currentNode.Position;
                    Vector2Int newPosition = new Vector2Int(currentPosition.x + dx[i], currentPosition.y + dy[i]);

                    if (IsValid(newPosition) && !openList.Select(node => node.Position).Contains(newPosition))
                    {
                        Node neighbor = new Node(newPosition);

                        if (closedList.Contains(neighbor))
                            continue;

                        neighbor.Parent = currentNode;
                        neighbor.CalculateEstimate(targetNode.Position);
                        neighbor.CalculateValue();

                        openList.Add(neighbor);
                    }
                }
            }
            path = new Vector2Int[0];
        }

        private bool IsValid(Vector2Int position)
        {
            if (this.endPoint == position)
                return true;

            if (!this.runtimeModel.IsTileWalkable(position))
                return false;

            foreach (var unit in runtimeModel.RoPlayerUnits)
            {
                if (unit.Pos == position)
                    return false;
            }

            foreach (var unit in runtimeModel.RoBotUnits)
            {
                if (unit.Pos == position)
                    return false;
            }

            return true;
        }

        private class Node
        {
            public Vector2Int Position;
            public int Cost = 10;
            public int Estimate;
            public int Value;
            public Node Parent;

            public Node(Vector2Int position)
            {
                Position = position;
            }

            public void CalculateEstimate(Vector2Int target)
            {
                int targetX = target.x;
                int targetY = target.y;
                Estimate = Math.Abs(Position.x - targetX) + Math.Abs(Position.y - targetY);
            }

            public void CalculateValue()
            {
                Value = Cost + Estimate;
            }

            public override bool Equals(object? obj)
            {
                if (obj is not Node node)
                {
                    return false;
                }

                return Position == node.Position;
            }
        }
    }
}
