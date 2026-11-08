using Model;
using Model.Runtime.Projectiles;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    public class DefaultPlayerUnitBrain : BaseUnitBrain
    {
        // Свой путь игрока (A*)
        private AStarUnitPath _playerPath;

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

        public override Vector2Int GetNextStep()
        {
            var coord = PlayerUnitCoordinator.Instance;
            if (!coord.RecommendedTarget.HasValue)
                return unit.Pos;

            var target = coord.RecommendedTarget.Value;

            // В радиусе атаки — стоим и стреляем.
            if (IsTargetInRange(target))
                return unit.Pos;

            // Путь строим прямо к цели.
            // на ближайшей проходимой клетке рядом с ней.
            if (_playerPath == null || _playerPath.EndPoint != target)
                _playerPath = new AStarUnitPath(runtimeModel, unit.Pos, target);

            var next = _playerPath.GetNextStepFrom(unit.Pos);

            // Не толкаемся в клетку, занятую другим юнитом.
            if (runtimeModel.RoUnits.Any(u => u != unit && u.Pos == next))
                return unit.Pos;

            return next;
        }

        protected override List<Vector2Int> SelectTargets()
        {
            var result = new List<Vector2Int>();
            var coord = PlayerUnitCoordinator.Instance;

            if (!coord.RecommendedTarget.HasValue)
                return result;

            var target = coord.RecommendedTarget.Value;
            float twoRanges = unit.Config.AttackRange * 2f;
            var diff = target - unit.Pos;
            if (diff.sqrMagnitude <= twoRanges * twoRanges)
                result.Add(target);

            return result;
        }

    }
}