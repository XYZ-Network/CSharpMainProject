using Model;
using Model.Runtime;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    public class DefaultPlayerUnitBrain : BaseUnitBrain
    {
        public override void Update(float deltaTime, float time)
        {
            base.Update(deltaTime, time);
            var recommendedTarget = UnitCoordinator.Instance.GetRecommendedTarget();
            var recommendedPosition = UnitCoordinator.Instance.GetRecommendedPosition();

            MoveTowards(recommendedPosition);
            AttackTarget(recommendedTarget);
        }

        protected float DistanceToOwnBase(Vector2Int fromPos)
        {
            return Vector2Int.Distance(fromPos, runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]);
        }

        private void MoveTowards(Vector2Int targetPos)
        {
        }

        private void AttackTarget(Vector2Int targetPos)
        {
        }
    }
}

