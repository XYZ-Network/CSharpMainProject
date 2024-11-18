using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using UnitBrains.Pathfinding;
using UnityEngine;

namespace UnitBrains.Player
{
    public class DefaultPlayerUnitBrain : BaseUnitBrain
    {
        private RuntimeModel runtimeModel;

        protected void Awake()
        {
            runtimeModel = Locator.Get<RuntimeModel>(); // Инициализация runtimeModel
            if (runtimeModel == null)
            {
                Debug.LogError("Failed to retrieve RuntimeModel from Locator.");
            }
            else
            {
                Debug.Log("RuntimeModel successfully initialized.");
            }
        }

        protected float DistanceToOwnBase(Vector2Int fromPos)
        {
            if (runtimeModel?.RoMap?.Bases == null)
            {
                Debug.LogWarning("Bases array is null.");
                return float.MaxValue; // Или любое другое значение по умолчанию
            }

            if (RuntimeModel.PlayerId < 0 || RuntimeModel.PlayerId >= runtimeModel.RoMap.Bases.Count)
            {
                Debug.LogWarning($"PlayerId is out of bounds. PlayerId: {RuntimeModel.PlayerId}, Bases Count: {runtimeModel.RoMap.Bases.Count}");
                return float.MaxValue; // Или любое другое значение по умолчанию
            }

            return Vector2Int.Distance(fromPos, runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]);
        }

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
            if (unit == null)
            {
                Debug.LogError("Unit is not initialized!");
                return Vector2Int.zero; // Или любое значение по умолчанию
            }

            var target = GetRecommendedTarget(); // Вызываем новый метод

            if (Vector2Int.Distance(unit.Pos, target) > 5f)
            {
                // Логика движения если цель далеко
                target = base.GetNextStep();
                Debug.LogWarning("No reaction");
            }
            else
            {
                Debug.LogWarning("Singleton reaction");
            }

            if (HasTargetsInRange())
                return unit.Pos;

           
            int x = unit.Pos.x; // Используем текущие координаты юнита
            int y = unit.Pos.y; // Используем текущие координаты юнита

            BaseUnitPath _activePath = new AStarUnitPath(x, y, runtimeModel, unit.Pos, target);

            return _activePath.GetNextStepFrom(unit.Pos);
        }

        private Vector2Int GetRecommendedTarget()
        {
            if (runtimeModel == null || runtimeModel.RoBotUnits == null || !runtimeModel.RoBotUnits.Any())
            {
                Debug.LogWarning("No enemies found or runtimeModel is not initialized!");
                return Vector2Int.zero;
            }

            var enemies = runtimeModel.RoBotUnits.ToList();
            return enemies.OrderBy(e => Vector2Int.Distance(unit.Pos, e.Pos)).FirstOrDefault()?.Pos ?? Vector2Int.zero;
        }
    }
}