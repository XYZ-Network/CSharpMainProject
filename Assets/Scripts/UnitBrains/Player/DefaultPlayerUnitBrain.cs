using System.Collections.Generic;
using System.Linq;
<<<<<<< HEAD
using System;
=======
>>>>>>> bd36a1f9d87e539756da4b144fb2f218062bc3e6
using Model;
using Model.Runtime;
using Model.Runtime.Projectiles;
using UnitBrains.Pathfinding;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    public class DefaultPlayerUnitBrain : BaseUnitBrain
    {
<<<<<<< HEAD
        // Поля для хранения runtimeModel и юнита.
        private IReadOnlyRuntimeModel _runtimeModel;
        private Unit _unit;

        // Конструктор, принимающий юнит и runtimeModel
        public DefaultPlayerUnitBrain(Unit unit, IReadOnlyRuntimeModel runtimeModel)
        {
            _unit = unit ?? throw new ArgumentNullException(nameof(unit), "Unit cannot be null.");
            _runtimeModel = runtimeModel ?? throw new ArgumentNullException(nameof(runtimeModel), "RuntimeModel cannot be null.");
        }

        // Метод для инициализации (можно вызывать вручную)
        public void Initialize()
        {
            Debug.Log("RuntimeModel successfully initialized.");
=======
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
>>>>>>> bd36a1f9d87e539756da4b144fb2f218062bc3e6
        }

        protected float DistanceToOwnBase(Vector2Int fromPos)
        {
<<<<<<< HEAD
            if (_runtimeModel?.RoMap?.Bases == null)
=======
            if (runtimeModel?.RoMap?.Bases == null)
>>>>>>> bd36a1f9d87e539756da4b144fb2f218062bc3e6
            {
                Debug.LogWarning("Bases array is null.");
                return float.MaxValue; // Или любое другое значение по умолчанию
            }

<<<<<<< HEAD
            if (RuntimeModel.PlayerId < 0 || RuntimeModel.PlayerId >= _runtimeModel.RoMap.Bases.Count)
            {
                Debug.LogWarning($"PlayerId is out of bounds. PlayerId: {RuntimeModel.PlayerId}, Bases Count: {_runtimeModel.RoMap.Bases.Count}");
                return float.MaxValue; // Или любое другое значение по умолчанию
            }

            return Vector2Int.Distance(fromPos, _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]);
=======
            if (RuntimeModel.PlayerId < 0 || RuntimeModel.PlayerId >= runtimeModel.RoMap.Bases.Count)
            {
                Debug.LogWarning($"PlayerId is out of bounds. PlayerId: {RuntimeModel.PlayerId}, Bases Count: {runtimeModel.RoMap.Bases.Count}");
                return float.MaxValue; // Или любое другое значение по умолчанию
            }

            return Vector2Int.Distance(fromPos, runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]);
>>>>>>> bd36a1f9d87e539756da4b144fb2f218062bc3e6
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
<<<<<<< HEAD
            if (_unit == null)
=======
            if (unit == null)
>>>>>>> bd36a1f9d87e539756da4b144fb2f218062bc3e6
            {
                Debug.LogError("Unit is not initialized!");
                return Vector2Int.zero; // Или любое значение по умолчанию
            }

<<<<<<< HEAD
            var target = GetRecommendedTarget();

            // Если цель далеко, можно добавить логику для безопасного подхода к цели
            if (Vector2Int.Distance(_unit.Pos, target) > 5f)
            {
                Debug.LogWarning("Target is too far away. Moving towards the target's last known position or performing an alternative action.");
                // Здесь можно добавить логику для движения к цели
                return _unit.Pos; // Вернем текущую позицию или альтернативное действие
            }

            if (HasTargetsInRange())
                return _unit.Pos;

            int x = _unit.Pos.x;
            int y = _unit.Pos.y;

            // Передаем информацию о юните и runtimeModel в AStarUnitPath
            BaseUnitPath activePath = new AStarUnitPath(x, y, _runtimeModel, _unit.Pos, target);
            return activePath.GetNextStepFrom(_unit.Pos);
=======
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
>>>>>>> bd36a1f9d87e539756da4b144fb2f218062bc3e6
        }

        private Vector2Int GetRecommendedTarget()
        {
<<<<<<< HEAD
            if (_runtimeModel == null || _runtimeModel.RoBotUnits == null || !_runtimeModel.RoBotUnits.Any())
            {
                Debug.LogWarning("No enemies found or runtimeModel is not initialized!");
                return _unit.Pos; // Возвращаем текущую позицию юнита, если врагов нет
            }

            var enemies = _runtimeModel.RoBotUnits.ToList();
            return enemies.OrderBy(e => Vector2Int.Distance(_unit.Pos, e.Pos)).FirstOrDefault()?.Pos ?? _unit.Pos;
        }

        public interface IPlayerUnitBrain
        {
            Vector2Int GetNextStep();
            void Initialize();
=======
            if (runtimeModel == null || runtimeModel.RoBotUnits == null || !runtimeModel.RoBotUnits.Any())
            {
                Debug.LogWarning("No enemies found or runtimeModel is not initialized!");
                return Vector2Int.zero;
            }

            var enemies = runtimeModel.RoBotUnits.ToList();
            return enemies.OrderBy(e => Vector2Int.Distance(unit.Pos, e.Pos)).FirstOrDefault()?.Pos ?? Vector2Int.zero;
>>>>>>> bd36a1f9d87e539756da4b144fb2f218062bc3e6
        }
    }
}