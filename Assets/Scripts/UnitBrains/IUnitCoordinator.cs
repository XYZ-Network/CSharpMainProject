using UnityEngine;

namespace UnitBrains
{
    public interface IReadOnlyUnitCoordinator
    {
        public Vector2Int RecommendedTarget { get; }
        public Vector2Int RecommendedPoint { get; }
    }
}