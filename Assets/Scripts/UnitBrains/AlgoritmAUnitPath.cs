using Model;
using Model.Runtime.ReadOnly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UnitBrains;
using UnitBrains.Pathfinding;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    public class AlgoritmAUnitPath : BaseUnitPath
    {

        private int[] dx = { -1, 0, 1, 0 };
        private int[] dy = { 0, 1, 0, -1 };
        private bool _isTargetFound;
        private bool _isEnemyUnitClose;
        private Node _nextToEnemyUnit;

        public AlgoritmAUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint) :
            base(runtimeModel, startPoint, endPoint)
        {
        }

        protected override void Calculate()
        {
            Node startNode = new Node(startPoint);
            Node targetNode = new Node(endPoint);

            List<Node> openList = new List<Node>() { startNode };
            List<Node> closedList = new List<Node>();

            int counter = 0;
            int maxCount = runtimeModel.RoMap.Width * runtimeModel.RoMap.Height;

            while (openList.Count > 0 && counter++ < maxCount)
            {
                Node currentNode = openList[0];

                foreach (var node in openList)
                {
                    if (node.Value < currentNode.Value)
                        currentNode = node;
                }
                openList.Remove(currentNode);
                closedList.Add(currentNode);

                if (_isTargetFound)
                {
                    path = FindPath(currentNode);
                    return;
                }

                for (int i = 0; i < dx.Length; i++)
                {
                    int newX = currentNode.Pos.x + dx[i];
                    int newY = currentNode.Pos.y + dy[i];

                    Vector2Int newPos = new Vector2Int(newX, newY);

                    if (newPos == targetNode.Pos)
                        _isTargetFound = true;


                    if (runtimeModel.IsTileWalkable(newPos) || _isTargetFound)
                    {
                        Node neighbor = new Node(newPos);

                        if (closedList.Contains(neighbor))
                            continue;

                        neighbor.Parent = currentNode;
                        neighbor.CalculateEstimate(targetNode.Pos);
                        neighbor.CalculateValue();

                        openList.Add(neighbor);
                    }

                    if (CheckEncounterWithEnemy(newPos) && !_isEnemyUnitClose)
                    {
                        _isEnemyUnitClose = true;
                        _nextToEnemyUnit = currentNode;
                    }
                }
            }

            if (_isEnemyUnitClose)
            {
                path = FindPath(_nextToEnemyUnit);
                return;
            }

            path = new Vector2Int[] { startNode.Pos };
        }

        private bool CheckEncounterWithEnemy(Vector2Int newPos)
        {
            var botUnitPositions = runtimeModel.RoBotUnits.Select(u => u.Pos).Where(u => u == newPos);

            return botUnitPositions.Any();
        }

        private Vector2Int[] FindPath(Node node)
        {
            List<Vector2Int> path = new();

            while (node != null)
            {
                path.Add(node.Pos);
                node = node.Parent;
            }

            path.Reverse();
            return path.ToArray();
        }
    }
}