using System.Collections.Generic;
using Assets.Scripts.Utilities;
using Model;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using UnitBrains.Pathfinding;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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

        public override Vector2Int GetNextStep()
        {
            //Если есть юниты для атаки - атакуем
            if (HasTargetsInRange())
                return unit.Pos;

            //если есть рекомендованая цель - идем к ней по пути и атакуем
            IReadOnlyUnit recomendTarget = _unitsTargetManager.recomendTarget;
            if (recomendTarget != null)
            {
                AStarUnitPath path = new AStarUnitPath(runtimeModel, unit.Pos, recomendTarget.Pos);
                return path.GetNextStepFrom(unit.Pos);
            }

            //в остальных случаях отрабатываем стандартный метод
            return base.GetNextStep();

        }

    }
}