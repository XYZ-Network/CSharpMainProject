using System.Collections.Generic;
using System.Linq;
using Model;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    public class AStarUnitPath : BaseUnitPath
    {
        private static readonly Vector2Int[] NeighborOffsets =
        {
            new(-1, -1),
            new(-1, 0),
            new(-1, 1),
            new(0, -1),
            new(0, 1),
            new(1, -1),
            new(1, 0),
            new(1, 1),
        };

        public AStarUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint)
            : base(runtimeModel, startPoint, endPoint)
        {
        }

        protected override void Calculate()
        {
            var targets = GetCandidateTargets();
            if (targets.Count == 0)
            {
                path = NormalizePath(null);
                return;
            }

            Vector2Int[] bestPath = null;
            var bestDistanceToTarget = int.MaxValue;

            foreach (var target in targets)
            {
                var candidatePath = TryBuildPath(target);
                if (candidatePath == null)
                    continue;

                var distanceToTarget = Heuristic(target, endPoint);
                if (bestPath == null ||
                    distanceToTarget < bestDistanceToTarget ||
                    distanceToTarget == bestDistanceToTarget && candidatePath.Length < bestPath.Length)
                {
                    bestPath = candidatePath;
                    bestDistanceToTarget = distanceToTarget;

                    if (distanceToTarget == 0)
                        break;
                }
            }

            path = NormalizePath(bestPath);
        }

        private List<Vector2Int> GetCandidateTargets()
        {
            var candidates = new List<Vector2Int>();
            for (int x = 0; x < runtimeModel.RoMap.Width; x++)
            {
                for (int y = 0; y < runtimeModel.RoMap.Height; y++)
                {
                    var cell = new Vector2Int(x, y);
                    if (!IsWalkable(cell))
                        continue;

                    candidates.Add(cell);
                }
            }

            return candidates
                .OrderBy(cell => Heuristic(cell, endPoint))
                .ThenBy(cell => (cell - startPoint).sqrMagnitude)
                .ToList();
        }

        private Vector2Int[] TryBuildPath(Vector2Int target)
        {
            var openSet = new List<Vector2Int> { startPoint };
            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            var gScore = new Dictionary<Vector2Int, int> { [startPoint] = 0 };
            var fScore = new Dictionary<Vector2Int, int> { [startPoint] = Heuristic(startPoint, target) };
            var closedSet = new HashSet<Vector2Int>();

            while (openSet.Count > 0)
            {
                var current = GetBestOpenNode(openSet, fScore, gScore);
                if (current == target)
                    return RestorePath(cameFrom, current);

                openSet.Remove(current);
                closedSet.Add(current);

                foreach (var neighbor in GetNeighbors(current))
                {
                    if (closedSet.Contains(neighbor))
                        continue;

                    var tentativeGScore = gScore[current] + MoveCost(current, neighbor);
                    if (gScore.TryGetValue(neighbor, out var existingGScore) && tentativeGScore >= existingGScore)
                        continue;

                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = tentativeGScore + Heuristic(neighbor, target);

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }

            return null;
        }

        private IEnumerable<Vector2Int> GetNeighbors(Vector2Int current)
        {
            foreach (var offset in NeighborOffsets)
            {
                var next = current + offset;
                if (!IsWalkable(next))
                    continue;

                var isDiagonal = offset.x != 0 && offset.y != 0;
                if (isDiagonal)
                {
                    var horizontal = current + new Vector2Int(offset.x, 0);
                    var vertical = current + new Vector2Int(0, offset.y);
                    if (!IsWalkable(horizontal) || !IsWalkable(vertical))
                        continue;
                }

                yield return next;
            }
        }

        private bool IsWalkable(Vector2Int cell)
        {
            return cell == startPoint || runtimeModel.IsTileWalkable(cell);
        }

        private static int MoveCost(Vector2Int from, Vector2Int to)
        {
            var diff = to - from;
            return diff.x != 0 && diff.y != 0 ? 14 : 10;
        }

        private static int Heuristic(Vector2Int from, Vector2Int to)
        {
            var dx = Mathf.Abs(from.x - to.x);
            var dy = Mathf.Abs(from.y - to.y);
            var diagonal = Mathf.Min(dx, dy);
            var straight = Mathf.Max(dx, dy) - diagonal;
            return diagonal * 14 + straight * 10;
        }

        private static Vector2Int GetBestOpenNode(
            List<Vector2Int> openSet,
            IReadOnlyDictionary<Vector2Int, int> fScore,
            IReadOnlyDictionary<Vector2Int, int> gScore)
        {
            var best = openSet[0];
            var bestF = fScore.TryGetValue(best, out var startF) ? startF : int.MaxValue;
            var bestG = gScore.TryGetValue(best, out var startG) ? startG : int.MaxValue;

            for (int i = 1; i < openSet.Count; i++)
            {
                var candidate = openSet[i];
                var candidateF = fScore.TryGetValue(candidate, out var f) ? f : int.MaxValue;
                var candidateG = gScore.TryGetValue(candidate, out var g) ? g : int.MaxValue;
                if (candidateF < bestF || candidateF == bestF && candidateG < bestG)
                {
                    best = candidate;
                    bestF = candidateF;
                    bestG = candidateG;
                }
            }

            return best;
        }

        private static Vector2Int[] RestorePath(
            IReadOnlyDictionary<Vector2Int, Vector2Int> cameFrom,
            Vector2Int current)
        {
            var result = new List<Vector2Int> { current };
            while (cameFrom.TryGetValue(current, out var previous))
            {
                current = previous;
                result.Add(current);
            }

            result.Reverse();
            return result.ToArray();
        }

        private Vector2Int[] NormalizePath(Vector2Int[] candidatePath)
        {
            if (candidatePath == null || candidatePath.Length == 0)
                return new[] { startPoint, startPoint };

            if (candidatePath.Length == 1)
                return new[] { candidatePath[0], candidatePath[0] };

            return candidatePath;
        }
    }
}
