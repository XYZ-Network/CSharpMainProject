using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    public class PlayerUnitCoordinator : MonoBehaviour
    {
        private Vector2Int _recommendedTarget;
        private List<Vector2Int> _recommendedRegion;

        public Vector2Int RecommendedTarget => _recommendedTarget;
        public List<Vector2Int> RecommendedRegion => _recommendedRegion;

        public void SetRecommendedTarget(Vector2Int target)
        {
            _recommendedTarget = target;
        }

        public void SetRecommendedRegion(List<Vector2Int> region)
        {
            _recommendedRegion = region;
        }
    }
}
