using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Codice.Client.Common.GameUI;
using Model;
using UnitBrains.Pathfinding;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static Codice.CM.Common.CmCallContext;
using static UnityEngine.GraphicsBuffer;

namespace Assets.Scripts.UnitBrains.Player
{
    public class Node // класс нод
    {
        public Vector2Int Pos => _position;
        public int Value => _value;
        public Node Parents { get; set; }

        private int _cost = 10;
        private Vector2Int _position;
        private int _estimateToTarget;
        private int _value;


        public Node(Vector2Int position)
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
            if (obj is not Node node)
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
            Node startNode = new Node(startPoint);
            Node targetNode = new Node(endPoint);

            List<Node> openList = new List<Node> { startNode };
            List<Node> closedList = new List<Node>();

            var nodesNumber = 0;

            while (openList.Any() && nodesNumber++ < _maxPathLength)
            {
                Node currentNode = openList[0];

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


        private void CheckNeighborTiles(Node currentNode, Node targetNode, List<Node> openList, List<Node> closedList)
        {
            foreach (var direction in _directions)
            {
                Vector2Int newTilePos = currentNode.Pos + direction;

                if (newTilePos == targetNode.Pos)
                    _isTargetReached = true;

                if (runtimeModel.IsTileWalkable(newTilePos) || _isTargetReached)
                {
                    Node newNode = new Node(newTilePos);

                    if (closedList.Contains(newNode))
                        continue;

                    newNode.Parents = currentNode;
                    newNode.CalculateEstimate(targetNode.Pos);
                    newNode.CalculateValue();

                    openList.Add(newNode);
                }
            }
        }


        private Vector2Int[] BuildPath(Node currentNode)
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




}


