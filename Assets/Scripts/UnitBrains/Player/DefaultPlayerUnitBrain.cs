using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using UnitBrains.Pathfinding;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

// тут нужно переопределить логику поведения юнитов // переопределяем public virtual Vector2Int GetNextStep()
// если юнит находится в 3х клетках от рекомендации, то не реагировать на рекомендации
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
            var target = _recommendationsForUnitsSingleton.RecommendationTarget(); // 9.поменял на свойство поля

            if(Vector2Int.Distance(unit.Pos, target) > 5f)
            {
                target = base.GetNextStep();
                //Debug.Log("No reaction");
            } else
            {
                target = _recommendationsForUnitsSingleton.RecommendationTarget(); // 10.поменял на свойство поля
                //Debug.Log("Singleton reaction");
            }

            if (HasTargetsInRange())
                return unit.Pos;

            BaseUnitPath _activePath = new DummyUnitPath(runtimeModel, unit.Pos, target);
            //BaseUnitPath _activePath = new AStarPathFinding(runtimeModel, unit.Pos, target);
            
            return _activePath.GetNextStepFrom(unit.Pos);
        }

    }
}