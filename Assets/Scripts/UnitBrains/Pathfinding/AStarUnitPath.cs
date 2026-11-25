using Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnitBrains.Pathfinding;
using UnityEngine;

namespace Assets.Scripts.UnitBrains.Pathfinding
{


    public class AStarUnitPath : BaseUnitPath
    {
        private class Node
        {
            public Vector2Int Pos;
            public Node Parent;
            public int G;
            public int H;
            public int F => G + H;

            public Node(Vector2Int pos, Node parent = null)
            {
                Pos = pos;
                Parent = parent;
            }
        }

        public AStarUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint)
            : base(runtimeModel, startPoint, endPoint)
        {
        }

        protected override void Calculate()
        {
            Vector2Int actualTarget = FindClosestWalkable(endPoint);
            var result = FindPath(startPoint, actualTarget);

            if (result != null) path = result.ToArray();
            else path = new Vector2Int[] { startPoint };
        }

        private List<Vector2Int> FindPath(Vector2Int start, Vector2Int target)
        {
            var openList = new List<Node>();
            var closedList = new HashSet<Vector2Int>();

            var startNode = new Node(start);
            openList.Add(startNode);

            Node current = null;
            bool found = false;
            Node closestNode = startNode;

            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            while (openList.Count > 0)
            {
                current = openList[0];
                for (int i = 1; i < openList.Count; i++)
                {
                    if (openList[i].F < current.F || (openList[i].F == current.F && openList[i].H < current.H))
                        current = openList[i];
                }

                openList.Remove(current);
                closedList.Add(current.Pos);

                if (current.H < closestNode.H)
                    closestNode = current;

                if (current.Pos == target)
                {
                    found = true;
                    break;
                }

                foreach (var dir in directions)
                {
                    Vector2Int neighborPos = current.Pos + dir;
                    if (closedList.Contains(neighborPos)) continue;

                    if (IsWall(neighborPos) && neighborPos != target) continue;

                    int stepCost = 1;

                    if (neighborPos != target && IsUnitAt(neighborPos))
                    {
                        stepCost += 100;
                    }

                    int tentativeG = current.G + stepCost;
                    Node neighbor = openList.Find(n => n.Pos == neighborPos);

                    if (neighbor == null)
                    {
                        neighbor = new Node(neighborPos, current)
                        {
                            G = tentativeG,
                            H = Mathf.Abs(neighborPos.x - target.x) + Mathf.Abs(neighborPos.y - target.y) + UnityEngine.Random.Range(0, 3)
                        };
                        openList.Add(neighbor);
                    }
                    else if (tentativeG < neighbor.G)
                    {
                        neighbor.Parent = current;
                        neighbor.G = tentativeG;
                    }
                }
            }

            if (!found)
            {
                current = closestNode;
                if (current == startNode && current.Pos == start) return null;
            }

            var result = new List<Vector2Int>();
            while (current != null)
            {
                result.Add(current.Pos);
                current = current.Parent;
            }
            result.Reverse();
            return result;
        }

        private Vector2Int FindClosestWalkable(Vector2Int target)
        {
            if (!IsWall(target)) return target;
            for (int radius = 1; radius < 50; radius++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    for (int y = -radius; y <= radius; y++)
                    {
                        if (Mathf.Abs(x) + Mathf.Abs(y) != radius) continue;
                        Vector2Int checkPos = target + new Vector2Int(x, y);
                        if (!IsWall(checkPos)) return checkPos;
                    }
                }
            }
            return target;
        }

        private bool IsWall(Vector2Int pos)
        {
            try
            {
                if (IsUnitAt(pos)) return false;
                return !runtimeModel.IsTileWalkable(pos);
            }
            catch { return true; }
        }

        private bool IsUnitAt(Vector2Int pos) => runtimeModel.RoUnits.Any(u => u.Pos == pos);
    }




}