using Model;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    public static class MapUtils
    {
        private static readonly Vector2Int[] AllDirections =
        {
            new(0, 1),
            new(0, -1),
            new(1, 0),
            new(-1, 0),
            new(1, 1),
            new(1, -1),
            new(-1, 1),
            new(-1, -1),
        };

        /// <summary>
        /// Finds the best walkable adjacent tile around a given origin that is closest to a target position.
        /// </summary>
        /// <param name="model">The runtime model used to access map data.</param>
        /// <param name="origin">The central tile whose neighbors will be evaluated.</param>
        /// <param name="target">
        /// The destination tile used to evaluate heuristic distance from each neighbor.
        /// The tile itself does not need to be walkable or adjacent.
        /// </param>
        /// <returns>
        /// The walkable neighboring tile (including diagonals) that has the shortest estimated distance to the target.
        /// If no neighbor is walkable, returns the original <paramref name="origin"/>.
        /// </returns>
        /// <remarks>
        /// This method ignores units and only checks for map walkability via <c>model.RoMap[pos]</c>.
        /// Useful for determining exit points from a base or entry points into tactical zones.
        /// </remarks>
        public static Vector2Int FindBestAdjacentTileTowards(
            IReadOnlyRuntimeModel model,
            Vector2Int origin,
            Vector2Int target
        )
        {
            Vector2Int result = origin;
            float best = float.MaxValue;

            foreach (var dir in AllDirections)
            {
                Vector2Int neighbor = origin + dir;

                if (!IsWalkableIgnoringUnits(model, neighbor))
                    continue;

                float h = Heuristic(neighbor, target);
                if (h < best)
                {
                    result = neighbor;
                    best = h;
                }
            }

            return result;
        }

        private static bool IsWalkableIgnoringUnits(IReadOnlyRuntimeModel model, Vector2Int pos)
        {
            return model.RoMap[pos];
        }

        private static float Heuristic(Vector2Int a, Vector2Int b)
        {
            // As we allow diagonals, it's better to use euclid distance instead of manhattan
            return Vector2Int.Distance(a, b);
        }
    }
}