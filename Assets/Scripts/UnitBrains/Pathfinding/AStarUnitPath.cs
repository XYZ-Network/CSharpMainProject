using Model;
using System.Collections.Generic;
using UnityEngine;


namespace UnitBrains.Pathfinding
{
    public class AStarUnitPath : BaseUnitPath
    {
        const int StraightCost = 10;
        const int DiagonalCost = 14;
        public AStarUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint) : base(runtimeModel, startPoint, endPoint)
        {
        }

        protected override void Calculate()
        {
            var openNodes = new List<Node>();
            var closedNodes = new List<Node>();
            Node startNode = new Node(startPoint);
            startNode.ValueFromStartPoint = 0;
            Node endNode = new Node(endPoint);
            startNode.ValueToEndPoint = CalculateHeuristic(startNode, endNode);
            openNodes.Add(startNode);
            Node foundNode = null;

            while (openNodes.Count > 0)
            {
                Node currentNode = openNodes[0];
                foreach (Node node in openNodes)
                {
                    if (node.CalculateValue() < currentNode.CalculateValue())
                        currentNode = node;
                }
                if (currentNode.Position == endNode.Position)
                {
                    foundNode = currentNode;
                    break;
                }

                openNodes.Remove(currentNode);
                closedNodes.Add(currentNode);

                for (int x = -1; x < 2; x++)
                {
                    for (int y = -1; y < 2; y++)
                    {
                        if (x == 0 && y == 0)
                            continue;
                        Vector2Int neighborPosition = new Vector2Int(currentNode.Position.x + x, currentNode.Position.y + y);
                        if (!runtimeModel.IsTileWalkable(neighborPosition) && neighborPosition != endPoint)
                            continue;
                        bool neighborIsClosed = false;
                        foreach (Node node in closedNodes)
                        {
                            if (node.Position == neighborPosition)
                            {  neighborIsClosed = true; 
                               break; 
                            }
                        }
                        if (neighborIsClosed) continue;
                        Node neighborNode = new Node(neighborPosition);
                        Node existingNode = null;
                        foreach (Node node in openNodes)
                        {
                            if (node.Position == neighborNode.Position)
                            {
                                existingNode = node;
                                break;
                            }
                        }
                        if (existingNode != null)
                        {
                            int newValueFromStartPoint = currentNode.ValueFromStartPoint + GetStepCost(currentNode, existingNode);
                            if (newValueFromStartPoint < existingNode.ValueFromStartPoint)
                            {
                                existingNode.ValueFromStartPoint = newValueFromStartPoint;
                                existingNode.Parent = currentNode;
                            }
                        }
                        else
                        {
                            neighborNode.ValueFromStartPoint = currentNode.ValueFromStartPoint + GetStepCost(currentNode, neighborNode);
                            neighborNode.ValueToEndPoint = CalculateHeuristic(neighborNode, endNode);
                            neighborNode.Parent = currentNode;
                            openNodes.Add(neighborNode);
                        }
                    }
                }
            }
            var aStarPath = new List<Vector2Int>();
            Node current = foundNode;
            if (current == null) 
            {
                aStarPath.Add(startPoint);
            }
            while (current != null)
            {
                aStarPath.Add(current.Position);
                current = current.Parent;
            }
            aStarPath.Reverse();
            path = aStarPath.ToArray();

        }

        private int GetStepCost(Node from, Node to)
        {
            var diffX = to.Position.x - from.Position.x;
            var diffY = to.Position.y - from.Position.y;
            if (diffX != 0 && diffY != 0)
            {
                return DiagonalCost;
            }
            return StraightCost;
        }

        private int CalculateHeuristic(Node from, Node to)
        {
            int absDiffX = Mathf.Abs(to.Position.x - from.Position.x);
            int absDiffY = Mathf.Abs(to.Position.y - from.Position.y);
            int diagonalSteps = Mathf.Min(absDiffX, absDiffY);
            int straightSteps = Mathf.Max(absDiffX, absDiffY) - diagonalSteps;

            return straightSteps * StraightCost + diagonalSteps * DiagonalCost;
        }
    }

    internal class Node
    {
        public Vector2Int Position;
        public int ValueFromStartPoint;
        public int ValueToEndPoint;
        public Node Parent;

        public Node(Vector2Int position)
        {
            Position = position;
        }

        public int CalculateValue()
        {
            return ValueFromStartPoint + ValueToEndPoint;
        }
    }
}