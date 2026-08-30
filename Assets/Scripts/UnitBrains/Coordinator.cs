using Model;
using Model.Runtime.ReadOnly;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;

namespace Assets.Scripts.UnitBrains
{
    public class Coordinator
    {
        private static Coordinator _instance;

        private IReadOnlyRuntimeModel _runtimeModel;
        private TimeUtil _timeUtil;

        private Vector2Int _basePos;
        private IEnumerable<IReadOnlyUnit> _enemyUnits;

        // Приватный конструктор синглтона
        private Coordinator()
        {
            // 1. Получаем ссылки
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
            _timeUtil = ServiceLocator.Get<TimeUtil>();

            // 2. БЕЗОПАСНО инициализируем позицию базы, так как _runtimeModel уже существует
            _basePos = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];

            // 3. Подписываемся на обновления
            _timeUtil.AddFixedUpdateAction(OnFixedUpdate);

            // 4. Первоначальный сбор врагов
            UpdateEnemyList();
        }

        public static Coordinator GetInstance()
        {
            if (_instance == null)
                _instance = new Coordinator();
            return _instance;
        }

        // Обновляем список врагов каждый кадр (как в правильном коде)
        private void OnFixedUpdate(float fixedDeltaTime)
        {
            UpdateEnemyList();
        }

        private void UpdateEnemyList()
        {
            // Берем всех юнитов на карте и фильтруем только вражеских (ботов)
            _enemyUnits = _runtimeModel.RoUnits.Where(u => u.Config.IsPlayerUnit == false);
        }

        private bool HasTargetsNearBase() // враги на твоей половине
        {
            if (!_enemyUnits.Any()) return false;

            return NearestEnemy().x < _runtimeModel.RoMap.Width / 2;
        }

        private Vector2Int MostDamagedTarget() // меньше всего хп
        {
            // Если врагов нет, идем к базе врага
            if (!_enemyUnits.Any())
                return _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];

            int lowestHealth = int.MaxValue;
            Vector2Int mostDamagedTarget = Vector2Int.zero;

            foreach (var unit in _enemyUnits)
            {
                if (unit.Health < lowestHealth)
                {
                    lowestHealth = unit.Health;
                    mostDamagedTarget = unit.Pos;
                }
            }
            return mostDamagedTarget;
        }

        private Vector2Int NearestEnemy() // ближайший враг
        {
            if (!_enemyUnits.Any())
                return _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];

            int minDistance = int.MaxValue;
            Vector2Int nearestTarget = Vector2Int.zero;

            foreach (var enemy in _enemyUnits)
            {
                var diff = enemy.Pos - _basePos;
                if (diff.sqrMagnitude < minDistance)
                {
                    minDistance = diff.sqrMagnitude;
                    nearestTarget = enemy.Pos;
                }
            }
            return nearestTarget;
        }

        public Vector2Int GetRecomendedTarget()
        {
            if (HasTargetsNearBase())
                return NearestEnemy();
            else
                return MostDamagedTarget();
        }

        // ВАЖНО: передаем дальность атаки в метод, так как глобальный Координатор не знает, какой юнит его вызывает
        public Vector2Int GetRecomendedPos(float attackRange)
        {
            if (HasTargetsNearBase())
            {
                return new Vector2Int(_basePos.x + 1, _basePos.y);
            }
            else
            {
                Vector2Int enemyPos = NearestEnemy();
                return new Vector2Int(enemyPos.x - (int)Math.Ceiling(attackRange), enemyPos.y);
            }
        }
    }
}