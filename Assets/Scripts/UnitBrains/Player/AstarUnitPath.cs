using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using UnitBrains.Pathfinding;
using UnityEngine;

public class PathNode
{
    public Vector2Int Pos => _position;
    public int Value => _value;
    public PathNode Parents { get; set; }

    private int _cost = 10;
    private Vector2Int _position;
    private int _estimateToTarget;
    private int _value;

    public PathNode(Vector2Int position)
    {
        _position = position;
    }

    public void CalculateEstimate(Vector2Int targetPos)
    {
        _estimateToTarget = Math.Abs(_position.x - targetPos.x) + Math.Abs(_position.y - targetPos.y);
    }

    public void CalculateValue()
    {
        _value = _cost + _estimateToTarget;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not PathNode node)
            return false;

        return _position.x == node.Pos.x && _position.y == node.Pos.y;
    }
}

public class AStarUnitPath : BaseUnitPath
{
    private int _maxPathLength = 100;
    private bool _isTargetReached;
    private Vector2Int[] _directions;

    public AStarUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint)
                        : base(runtimeModel, startPoint, endPoint)
    {
        _directions = new[]
        {
                Vector2Int.left,
                Vector2Int.up,
                Vector2Int.right,
                Vector2Int.down,
            };
        _isTargetReached = false;
    }

    protected override void Calculate()
    {
        PathNode startNode = new PathNode(startPoint);
        PathNode targetNode = new PathNode(endPoint);

        List<PathNode> openList = new List<PathNode> { startNode };
        List<PathNode> closedList = new List<PathNode>();

        var nodesNumber = 0;

        while (openList.Any() && nodesNumber++ < _maxPathLength)
        {
            PathNode currentNode = openList[0];

            foreach (var node in openList)
            {
                if (node.Value < currentNode.Value)
                    currentNode = node;
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);


            if (_isTargetReached)
            {
                path = BuildPath(currentNode);
                return;
            }

            CheckNeighborTiles(currentNode, targetNode, openList, closedList);
        }

        path = closedList.Select(node => node.Pos).ToArray();
    }

    private void CheckNeighborTiles(PathNode currentNode, PathNode targetNode, List<PathNode> openList, List<PathNode> closedList)
    {
        foreach (var direction in _directions)
        {
            Vector2Int newTilePos = currentNode.Pos + direction;

            if (newTilePos == targetNode.Pos)
                _isTargetReached = true;

            if (runtimeModel.IsTileWalkable(newTilePos) || _isTargetReached)
            {
                PathNode newNode = new PathNode(newTilePos);

                if (closedList.Contains(newNode))
                    continue;

                newNode.Parents = currentNode;
                newNode.CalculateEstimate(targetNode.Pos);
                newNode.CalculateValue();

                openList.Add(newNode);
            }
        }
    }

    private Vector2Int[] BuildPath(PathNode currentNode)
    {
        List<Vector2Int> path = new();

        while (currentNode != null)
        {
            path.Add(currentNode.Pos);
            currentNode = currentNode.Parents;
        }

        path.Reverse();
        return path.ToArray();
    }
}
//using System.Collections;
//using System.Collections.Generic;
//using Model;
//using UnitBrains.Pathfinding;
//using UnityEngine;
//using UnityEngine.Tilemaps;

//public class AstarUnitPath : BaseUnitPath
//{   
//    public AstarUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint) : base(runtimeModel, startPoint, endPoint)
//    {
//    }


//    private class Node
//    {
//        public Vector2Int Position;
//        public Node Parent;
//        public int Cost;
//        public int Estimate;
//        public int Value => Cost + Estimate;

//        public Node(Vector2Int pos)
//        {
//            Position = pos;
//        }
//    }        
//    private int Heuristic(Vector2Int a, Vector2Int b)
//    {
//        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
//    }

//    private List<Vector2Int> RetracePath(Node endNode)
//    {
//        List<Vector2Int> result = new();
//        Node current = endNode;
//        while (current != null)
//        {
//            result.Add(current.Position);
//            current = current.Parent;
//        }
//        result.Reverse();
//        return result;
//    }

//    private bool IsWalkable(Vector2Int pos)
//    {
//        return runtimeModel.IsTileWalkable(pos);
//    }

//    private List<Vector2Int> GetNeighbors(Vector2Int pos)
//    {
//        List<Vector2Int> neighbors = new();
//        Vector2Int[] directions = new Vector2Int[]
//        {
//            new Vector2Int(0, 1),
//            new Vector2Int(1, 0),
//            new Vector2Int(0, -1),
//            new Vector2Int(-1, 0),
//        };

//        foreach (var dir in directions)
//        {
//            Vector2Int neighborPos = pos + dir;
//            if(IsWalkable(neighborPos))
//                neighbors.Add(neighborPos);
//        }
//        return neighbors;
//    }

//    protected override void Calculate()
//    {
//        var map = runtimeModel.RoMap;
//        var openList = new List<Node>();
//        var closedSet = new HashSet<Vector2Int>();

//        Node startNode = new Node(startPoint) { Cost = 0, Estimate = Heuristic(startPoint, endPoint) };
//        openList.Add(startNode);

//        while (openList.Count > 0)
//        {
//            openList.Sort((a, b) => a.Value.CompareTo(b.Value));
//            Node current = openList[0];
//            openList.RemoveAt(0);

//            if (current.Position == endPoint)
//            {
//                path = RetracePath(current).ToArray();
//                return;
//            }            

//            closedSet.Add(current.Position);

//            foreach (var neighborPos in GetNeighbors(current.Position))
//            {
//                if (closedSet.Contains(neighborPos))
//                    continue;

//                int tentative = current.Cost + 1;
//                Node neighbor = openList.Find(n => n.Position == neighborPos);

//                if (neighbor == null)
//                {
//                    neighbor = new Node(neighborPos)
//                    {
//                        Cost = tentative,
//                        Estimate = Heuristic(neighborPos, endPoint),
//                        Parent = current
//                    };
//                    openList.Add(neighbor);
//                }
//                else if (tentative < neighbor.Cost)
//                {
//                    neighbor.Cost = tentative;
//                    neighbor.Parent = current;
//                }
//            }
//        }
//        path = new Vector2Int[0];
//    }
//}
