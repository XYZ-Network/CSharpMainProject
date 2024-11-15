using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
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
            // Получаем целевую позицию из UnitCoordinator
            Vector2Int target = UnitCoordinator.Instance.RecommendedTarget;

            // Проверяем расстояние до рекомендованной позиции
            if (Vector2Int.Distance(unit.Pos, target) <= 3f)
            {
                // Если юнит находится в пределах 3 клеток от рекомендации, не реагируем на нее
                Debug.Log("No reaction to recommendation, within 3 cells.");
                return unit.Pos; // Возвращаем текущее положение юнита, чтобы он не двигался
            }

            // Если юнит находится дальше, чем 5 клеток от целевой позиции, можно добавить свою логику
            if (Vector2Int.Distance(unit.Pos, target) > 5f)
            {
                Debug.Log("No reaction, moving to base.");
                // Здесь вы можете определить, что делать в этом случае
                target = base.GetNextStep(); // Можно двигаться к базе или другой логике
            }
            else
            {
                Debug.Log("Singleton reaction, moving towards target.");
            }

            // Если у юнита есть цели в пределах досягаемости, возвращаем текущее положение
            if (HasTargetsInRange())
                return unit.Pos;

            // Создаем путь к целевой позиции
            BaseUnitPath _activePath = new AStarUnitPath(runtimeModel, unit.Pos, target);
            return _activePath.GetNextStepFrom(unit.Pos);
        }

    }
}