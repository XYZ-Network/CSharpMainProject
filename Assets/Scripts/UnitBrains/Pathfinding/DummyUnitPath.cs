using System.Collections.Generic;
using System.Linq;
using Model;
using UnityEngine;
using Utilities;

namespace UnitBrains.Pathfinding
{
    public class DummyUnitPath : BaseUnitPath
    {
        private const int MaxLength = 100;

        private class Node
        {
            public Vector2Int Position;
            public float GCost;
            public float HCost;
            public float FCost => GCost + HCost;
            public Node Parent;

            public Node(Vector2Int position)
            {
                Position = position;
            }
        }

        public DummyUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint) : base(runtimeModel, startPoint, endPoint)
        {
        }

        protected override void Calculate()
        {
            var pathList = FindAStarPathWithDiagonal(startPoint, endPoint);

            if (pathList != null && pathList.Count > 1)
            {
                path = pathList.ToArray();
                Debug.Log($"A* SUCCESS with diagonal! Path length: {path.Length}");
                return;
            }

            // Fallback: простой путь
            path = new Vector2Int[] { startPoint };
            Debug.Log($"A* failed, using fallback");
        }

        // A* с 8 направлениями (включая диагонали)
        private List<Vector2Int> FindAStarPathWithDiagonal(Vector2Int start, Vector2Int target)
        {
            if (!runtimeModel.IsTileWalkable(start))
                return null;

            if (!runtimeModel.IsTileWalkable(target))
                return null;

            var openSet = new List<Node>();
            var closedSet = new HashSet<Vector2Int>();

            var startNode = new Node(start);
            startNode.GCost = 0;
            startNode.HCost = HeuristicDiagonal(start, target);
            openSet.Add(startNode);

            var iterations = 0;
            var maxIterations = 30000;

            while (openSet.Count > 0 && iterations < maxIterations)
            {
                iterations++;

                var current = openSet.OrderBy(n => n.FCost).First();

                if (current.Position == target)
                {
                    Debug.Log($"A* found path in {iterations} iterations");
                    return ReconstructPath(current);
                }

                openSet.Remove(current);
                closedSet.Add(current.Position);

                // 8 направлений: 4 кардинальных + 4 диагональных
                var neighbors = GetAllNeighbors(current.Position);

                foreach (var neighborPos in neighbors)
                {
                    if (!IsTilePassableForMovement(neighborPos))
                        continue;

                    if (closedSet.Contains(neighborPos))
                        continue;

                    // Стоимость: диагональ дороже (1.4), прямо - 1
                    bool isDiagonal = Mathf.Abs(neighborPos.x - current.Position.x) == 1 &&
                                      Mathf.Abs(neighborPos.y - current.Position.y) == 1;
                    float moveCost = isDiagonal ? 1.4f : 1f;

                    float tentativeGCost = current.GCost + moveCost;
                    var neighborNode = openSet.FirstOrDefault(n => n.Position == neighborPos);

                    if (neighborNode == null)
                    {
                        neighborNode = new Node(neighborPos);
                        neighborNode.GCost = tentativeGCost;
                        neighborNode.HCost = HeuristicDiagonal(neighborPos, target);
                        neighborNode.Parent = current;
                        openSet.Add(neighborNode);
                    }
                    else if (tentativeGCost < neighborNode.GCost)
                    {
                        neighborNode.GCost = tentativeGCost;
                        neighborNode.Parent = current;
                    }
                }
            }

            return null;
        }

        // Диагональная эвристика (чебышёвское расстояние)
        private float HeuristicDiagonal(Vector2Int a, Vector2Int b)
        {
            int dx = Mathf.Abs(a.x - b.x);
            int dy = Mathf.Abs(a.y - b.y);
            return Mathf.Max(dx, dy) + (Mathf.Min(dx, dy) * 0.5f);
        }

        // 8 направлений
        private List<Vector2Int> GetAllNeighbors(Vector2Int pos)
        {
            return new List<Vector2Int>
            {
                // Кардинальные направления
                pos + Vector2Int.up,
                pos + Vector2Int.down,
                pos + Vector2Int.left,
                pos + Vector2Int.right,
                // Диагональные направления
                pos + new Vector2Int(1, 1),
                pos + new Vector2Int(1, -1),
                pos + new Vector2Int(-1, 1),
                pos + new Vector2Int(-1, -1)
            };
        }

        private bool IsTilePassableForMovement(Vector2Int pos)
        {
            if (!runtimeModel.IsTileWalkable(pos))
                return false;

            var unitAtPos = runtimeModel.RoUnits.FirstOrDefault(u => u.Pos == pos);
            if (unitAtPos != null)
            {
                var ourUnit = runtimeModel.RoUnits.FirstOrDefault(u => u.Pos == startPoint);
                if (ourUnit != null && unitAtPos.Config.IsPlayerUnit == ourUnit.Config.IsPlayerUnit && unitAtPos != ourUnit)
                {
                    return false;
                }
            }

            return true;
        }

        private List<Vector2Int> ReconstructPath(Node endNode)
        {
            var pathList = new List<Vector2Int>();
            var current = endNode;

            while (current != null)
            {
                pathList.Add(current.Position);
                current = current.Parent;
            }

            pathList.Reverse();
            return pathList;
        }
    }
}