using System.Collections.Generic;
using System.Linq;
using System;
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
        }

        private RuntimeModel _localRuntimeModel; // Переименованное поле

        protected void Awake()
        {
            _localRuntimeModel = Locator.Get<RuntimeModel>(); // Инициализация runtimeModel
            if (_localRuntimeModel == null)
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
            if (_runtimeModel?.RoMap?.Bases == null)
            {
                Debug.LogWarning("Bases array is null.");
                return float.MaxValue; // Или любое другое значение по умолчанию
            }

            if (_runtimeModel.RoMap.Bases.Count == 0)
            {
                Debug.LogWarning("No bases available.");
                return float.MaxValue; // Или любое другое значение по умолчанию
            }

            // Проверяем PlayerId
            if (RuntimeModel.PlayerId < 0 || RuntimeModel.PlayerId >= _runtimeModel.RoMap.Bases.Count)
            {
                Debug.LogWarning($"PlayerId is out of bounds. PlayerId: {RuntimeModel.PlayerId}, Bases Count: {_runtimeModel.RoMap.Bases.Count}");
                return float.MaxValue; // Или любое другое значение по умолчанию
            }

            return Vector2Int.Distance(fromPos, _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]);
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
            if (_unit == null)
            {
                Debug.LogError("Unit is not initialized!");
                return Vector2Int.zero; // Или любое значение по умолчанию
            }

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
        }

        private Vector2Int GetRecommendedTarget()
        {
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
        }
    }
}