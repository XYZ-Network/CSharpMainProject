using UnityEngine;

namespace UnitBrains.Enemy
{
    public class BotUnitController : IReadOnlyUnitController
    {
        public Vector2Int RecommendedTarget { get; }
        public Vector2Int RecommendedPoint { get; }

        public BotUnitController() { }

        //add bot-coordinator implementation here
    }
}