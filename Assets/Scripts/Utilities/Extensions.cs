using UnityEngine;

namespace Utilities
{
    public static class Extensions
    {
        public static Vector2Int SignOrZero(this Vector2Int vector)
        {
            return new Vector2Int(
                vector.x > 0 ? 1 : vector.x < 0 ? -1 : 0,
                vector.y > 0 ? 1 : vector.y < 0 ? -1 : 0
            );
        }
    }
}