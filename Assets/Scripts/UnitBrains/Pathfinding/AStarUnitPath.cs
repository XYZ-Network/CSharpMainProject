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
    internal class AStarUnitPath : BaseUnitPath
    {
        private const int MaxCellsToCheck = 1600;
        public AStarUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint) : base(runtimeModel, startPoint, endPoint)
        {
        }

        private List<Vector2Int> _vectorsToCheck = new List<Vector2Int> { Vector2Int.right, Vector2Int.up, Vector2Int.down, Vector2Int.left };

        protected override void Calculate()

        {
            CalculatePath();
        }
        private void CalculatePath()
        {

            PathNode startPathNode = new PathNode(startPoint);
            PathNode targetPathNode = new PathNode(endPoint);

            List<PathNode> openList = new List<PathNode>() { startPathNode };
            List<PathNode> closedList = new List<PathNode>();
            int checkedCells = 0;
            while (openList.Count > 0)
            {
                PathNode currentPathNode = openList[0];
                foreach (var pathNode in openList)
                {
                    if (pathNode.Value < currentPathNode.Value)
                        currentPathNode = pathNode;
                }
                openList.Remove(currentPathNode);
                closedList.Add(currentPathNode);
                if (currentPathNode.Equals(targetPathNode)/* || checkedCells > MaxCellsToCheck*/)
                {
                    path = ConvertGraphToArray(currentPathNode);
                    return;
                }
                for (int i = 0; i < _vectorsToCheck.Count; i++)
                {
                    Vector2Int newPos = currentPathNode.Position + _vectorsToCheck[i];
                    if (runtimeModel.IsTileWalkable(newPos) || newPos.Equals(endPoint) || IsUnitAtPos(newPos))
                    {
                        PathNode neighbor;
                        if (runtimeModel.IsTileWalkable(newPos) || newPos.Equals(endPoint))
                        {
                            neighbor = new PathNode(newPos);
                        }
                        else
                        {
                            neighbor = new PathNode(newPos, 16);
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

        private bool IsUnitAtPos(Vector2Int pos) {
            return runtimeModel.RoUnits.Any(u => u.Pos == pos);
        }

        private Vector2Int[] ConvertGraphToArray(PathNode currentNode) {
            List<PathNode> calculatedPath = new List<PathNode>();
            while (currentNode != null)
            {
                calculatedPath.Add(currentNode);
                currentNode = currentNode.Parent;
            }
            calculatedPath.Reverse();
            return calculatedPath.Select(node => node.Position).ToArray();
        }

        private bool IsCloseToTarget(Vector2Int pos, Vector2Int target)
        {
            return (target - pos).sqrMagnitude < 2;
        }

    }
}
