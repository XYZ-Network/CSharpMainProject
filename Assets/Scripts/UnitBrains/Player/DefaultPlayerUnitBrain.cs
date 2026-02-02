using System.Collections.Generic;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;
using UnitBrains.Player;
using Utilities;

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

        // Используем рекомендованную координатором точку как цель движения (если координатор доступен)
        public override Vector2Int GetNextStepTarget()
        {
            if (ServiceLocator.Contains<IPlayerUnitsCoordinator>())
                return ServiceLocator.Get<IPlayerUnitsCoordinator>().RecommendedPoint;

            return base.GetNextStepTarget();
        }
    }
}