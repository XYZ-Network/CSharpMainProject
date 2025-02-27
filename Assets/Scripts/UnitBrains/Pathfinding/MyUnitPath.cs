using GluonGui.Dialog;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitBrains.Pathfinding;
using UnityEngine;
using Utilities;

namespace UnitBrains.Pathfinding
{
    public class MyUnitPath : BaseUnitPath
    {
        private Vector2Int[] neighborDimension = new Vector2Int[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        private bool IsPlayerUnitBrain;
        public MyUnitPath(IReadOnlyRuntimeModel runtimeModel, bool IsPlayerUnitBrain, Vector2Int startPoint, Vector2Int endPoint) : base(runtimeModel, startPoint, endPoint)
        {
            this.IsPlayerUnitBrain = IsPlayerUnitBrain;
        }

        protected override void Calculate()
        {
            path = FindPath().ToArray();
        }
        public List<Vector2Int> FindPath()
        {
            Node startNode = new Node(startPoint);
            Node targetNode = new Node(endPoint);
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
            
                for (int i = 0; i < neighborDimension.Length; i++)
                {
                    Vector2Int newPoint = currentNode.position + neighborDimension[i];
                    if (newPoint == targetNode.position)
                    {
                        List<Vector2Int> result = new List<Vector2Int>();

                        while (currentNode != null)
                        {
                            result.Add(currentNode.position);
                            currentNode = currentNode.Parent;
                        }

                        result.Reverse();
                        return result;
                    }
                    
                    if (runtimeModel.IsTileWalkable(newPoint) || IsEnemy(newPoint))
                    {
                        Node neighbor = new Node(newPoint);
                        bool hasNode = false;
                        foreach (var node in closedList)
                        {
                            if (node.position == neighbor.position) 
                                hasNode = true;
                        }
                        if(hasNode) continue;
                        neighbor.Parent = currentNode;
                        neighbor.CalculateEstimate(endPoint);
                        neighbor.CalculateValue();

                        openList.Add(neighbor);
                    }
                }
            }
            return new List<Vector2Int> { startPoint, startPoint };
        }
        public bool IsEnemy(Vector2Int point)
        {
            if (IsPlayerUnitBrain)
            {
                foreach (var enemy in runtimeModel.RoBotUnits)
                {
                    if (point == enemy.Pos) return true;
                }
            }
            else if (!IsPlayerUnitBrain)
            {
                foreach (var enemy in runtimeModel.RoPlayerUnits)
                {
                    if (point == enemy.Pos) return true;
                }
            }
            return false;
        }
    }
}
