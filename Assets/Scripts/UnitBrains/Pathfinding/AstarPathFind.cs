using Model;
using Model.Runtime.ReadOnly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace UnitBrains.Pathfinding
{
    internal class AstarPathFind : BaseUnitPath
    {
        
        private const int MaxCellsToCheck = 1600;
        public AstarPathFind(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint) : base(runtimeModel, startPoint, endPoint)
        {
        }

        private List<Vector2Int> _vectorsToCheck = new List<Vector2Int> { Vector2Int.right, Vector2Int.up, Vector2Int.down, Vector2Int.left };

        protected override void Calculate()

        {
            CalculatePath();
        }
        private void CalculatePath()
        {

            Node startPathNode = new Node(startPoint);
            Node targetPathNode = new Node(endPoint);

            List<Node> openList = new List<Node>() { startPathNode };
            List<Node> closedList = new List<Node>();
            int checkedCells = 0;
            while (openList.Count > 0)
            {
                Node currentPathNode = openList[0];
                foreach (var pathNode in openList)
                {
                    if (pathNode.Value < currentPathNode.Value)
                        currentPathNode = pathNode;
                }
                openList.Remove(currentPathNode);
                closedList.Add(currentPathNode);
                if (currentPathNode.Equals(targetPathNode))
                {
                    path = ConvertGraphToArray(currentPathNode);
                    return;
                }
                for (int i = 0; i < _vectorsToCheck.Count; i++)
                {
                    Vector2Int newPosition = currentPathNode.Position + _vectorsToCheck[i];
                    if (runtimeModel.IsTileWalkable(newPosition) || newPosition.Equals(endPoint) || IsUnitAtPos(newPosition))
                    {
                        Node neighbor;
                        if (runtimeModel.IsTileWalkable(newPosition) || newPosition.Equals(endPoint))
                        {
                            neighbor = new Node(newPosition);
                        }
                        else
                        {
                            neighbor = new Node(newPosition, 16);
                        }
                        if (closedList.Contains(neighbor))
                            continue;
                        neighbor.Parent = currentPathNode;
                        neighbor.CalculateEstimate(targetPathNode.Position);
                        neighbor.CalculateValue();
                        openList.Add(neighbor);
                    }
                    checkedCells++;
                }
            }
            path = null;
        }

        private bool IsUnitAtPos(Vector2Int Position)
        {
            return runtimeModel.RoUnits.Any(Unit => Unit.Pos == Position);
        }

        private Vector2Int[] ConvertGraphToArray(Node currentNode)
        {
            List<Node> calculatedPath = new List<Node>();
            while (currentNode != null)
            {
                calculatedPath.Add(currentNode);
                currentNode = currentNode.Parent;
            }
            calculatedPath.Reverse();
            return calculatedPath.Select(node => node.Position).ToArray();
        }


    }
}