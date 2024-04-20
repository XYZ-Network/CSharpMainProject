using UnityEngine;

namespace UnitBrains
{
    public interface IReadOnlyUnitController
    {
        public Vector2Int RecommendedTarget { get; }
        public Vector2Int RecommendedPoint { get; }
    }
}