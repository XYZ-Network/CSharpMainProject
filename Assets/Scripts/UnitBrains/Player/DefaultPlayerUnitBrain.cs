using System.Collections.Generic;
using Model.Runtime.Projectiles;
using UnityEngine;
using UnitBrains.Coordinator;
using Model;

namespace UnitBrains.Player
{
    public class DefaultPlayerUnitBrain : BaseUnitBrain
    {
        protected float DistanceToOwnBase(Vector2Int fromPos) =>
            Vector2Int.Distance(fromPos, runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]);

        protected void SortByDistanceToOwnBase(List<Vector2Int> list)
        {
            list.Sort(CompareByDistanceToOwnBase);
        }

        private int CompareByDistanceToOwnBase(Vector2Int a, Vector2Int b)
        {
            var distanceA = DistanceToOwnBase(a);
            var distanceB = DistanceToOwnBase(b);
            return distanceA.CompareTo(distanceB);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            var coordinator = UnitCoordinator.Instance;

            // Если координатор указал цель и она в пределах двойного радиуса атаки — атакуем
            if (coordinator.RecommendedTarget.HasValue &&
                IsWithinDoubleAttackRange(coordinator.RecommendedTarget.Value))
            {
                return new List<Vector2Int> { coordinator.RecommendedTarget.Value };
            }

            // Если нет цели, но есть точка — двигаемся к ней
            if (coordinator.RecommendedPoint.HasValue)
            {
                // Можно использовать это, чтобы повлиять на GetNextStep
                _targetsToMove = new List<Vector2Int> { coordinator.RecommendedPoint.Value };
            }

            // Ищем цели в досягаемости
            var result = GetReachableTargets();
            if (result.Count > 1)
                result.RemoveAt(result.Count - 1);

            return result;
        }


        public override Vector2Int GetNextStep()
        {
            var coordinator = UnitCoordinator.Instance;
            if (coordinator.RecommendedPoint.HasValue)
                return coordinator.RecommendedPoint.Value;

            return base.GetNextStep();
        }

        private bool IsWithinDoubleAttackRange(Vector2Int pos)
        {
            float range = unit.Config.AttackRange;
            return (pos - unit.Pos).sqrMagnitude <= range * range * 4;
        }
    }
}
