using UnityEngine;

namespace UnitBrains.Enemy
{
    public class BotUnitCoordinator : IReadOnlyUnitCoordinator
    {
        public Vector2Int RecommendedTarget { get; }
        public Vector2Int RecommendedPoint { get; }
        
        public BotUnitCoordinator(){ }
        
        //add bot-coordinator implementation here
    }
}