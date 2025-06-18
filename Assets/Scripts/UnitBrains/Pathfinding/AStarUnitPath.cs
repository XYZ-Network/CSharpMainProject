using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    
    public class Node
    {
        public Vector2Int Pos;
        public int Cost = 1;
        public int Estimation;
        public int Value;
        public Node Parent;

        public Node(Vector2Int pos)
        {
            Pos = pos;
        }

        public override bool Equals(object? obj)
        {
            if (obj is Node node)
                return Pos.x == node.Pos.x && Pos.y == node.Pos.y;

            return false;
        }
    }
    
    public class AStarUnitPath : BaseUnitPath
    {
        public AStarUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint) 
            : base(runtimeModel, startPoint, endPoint)
        {
        }

        protected override void Calculate()
        {
            Node startNode = new Node(startPoint);
            Node targetNode = new Node(endPoint);

            int maxDistance = ManhattanDistance(startNode, targetNode);
            int tryDistacne = 0;
            
            // if the path is blocked, let's try to find at least part of the path
            while ((path == null || path.Length == 0) && tryDistacne < maxDistance)
            {
                path = CalculatePath(startNode, targetNode, tryDistacne)
                    .Select(node => node.Pos)
                    .ToArray();
                tryDistacne++;
            }
        }

        public List<Node> CalculatePath(Node start, Node target, int distance)
        {
            var openList = new List<Node> { start };
            var closedList = new List<Node>();
            
            while (openList.Count > 0)
            {
                var current = openList
                    .OrderBy(n => n.Value)
                    .First();

                if (ManhattanDistance(current, target) <= distance)
                    return ReconstructPath(current);

                openList.Remove(current);
                closedList.Add(current);

                foreach (var neighbor in GetNeighbors(current))
                {
                    bool inOpen = openList.Contains(neighbor);
                    bool inClone = closedList.Contains(neighbor);
                    if (inOpen || inClone || !IsValid(neighbor)) 
                        continue;
                    
                    neighbor.Parent = current;
                    neighbor.Estimation = ManhattanDistance(neighbor, target);
                    neighbor.Value = neighbor.Cost + neighbor.Estimation;
                    openList.Add(neighbor);
                }
            }

            return new();
        }
        
        public int ManhattanDistance(Node a, Node b) => 
            Math.Abs(a.Pos.x - b.Pos.x) + Math.Abs(a.Pos.y - b.Pos.y);
        
        public List<Node> ReconstructPath(Node node)
        {
            var path = new List<Node>();
            while (node != null)
            {
                path.Add(node);
                node = node.Parent;
            }
            path.Reverse();
            return path;
        }
        
        public List<Node> GetNeighbors(Node node)
        {
            var neighbors = new List<Node>();
            Vector2Int[] directions =
            {
                Vector2Int.up, 
                Vector2Int.right, 
                Vector2Int.down, 
                Vector2Int.left
            };

            foreach (var dir in directions)
                neighbors.Add(new Node(node.Pos + dir));

            return neighbors;
        }

        public bool IsValid(Node node)
        {
            bool validX = node.Pos.x >= 0 && node.Pos.x < runtimeModel.RoMap.Width;
            bool validY = node.Pos.y >= 0 && node.Pos.y < runtimeModel.RoMap.Height;
            bool walkable = runtimeModel.IsTileWalkable(node.Pos);
            return validX && validY && walkable;
        }
    }
}