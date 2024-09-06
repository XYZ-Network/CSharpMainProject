using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using UnityEngine;
using System.Collections;
using UnitBrains;
using Model.Runtime.ReadOnly;

namespace Assets.Scripts.UnitBrains
{
    public class PathAndTargetCoordinator
    {
        private static PathAndTargetCoordinator _instance;

        private IReadOnlyRuntimeModel _runtimeModel;
        private TimeUtil _timeUtil;
        private Vector2Int? _priorityTargetPosition = null;
        private Vector2Int? _prioritySelfPosition = null;

        public Vector2Int? PriorityTargetPosition { get => _priorityTargetPosition; }
        public Vector2Int? PrioritySelfPosition { get => _prioritySelfPosition; }

        public PathAndTargetCoordinator()
        {
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
            _timeUtil = ServiceLocator.Get<TimeUtil>();

            _timeUtil.AddFixedUpdateAction(getPriorityTargetPosition);
            _timeUtil.AddFixedUpdateAction(getPrioritySelfPosition);
        }

        
        public void getPriorityTargetPosition(float deltaTime)
        {
            List<Vector2Int> enemiesCloseToBase = getEnemiesCloseToBase();
            if (enemiesCloseToBase.Count > 0)
            {
                _priorityTargetPosition = enemiesCloseToBase[0];
                return;
            }

            List<Vector2Int> enemiesWithLowHealth = getEnemiesWithLowHealth();
            if (enemiesWithLowHealth.Count > 0)
            {
                _priorityTargetPosition = enemiesWithLowHealth[0];
                return;

            }

            _priorityTargetPosition = null;
        }

        public void getPrioritySelfPosition(float deltaTime)
        {
            List<Vector2Int> enemiesCloseToBase = getEnemiesCloseToBase();

            if (enemiesCloseToBase.Count > 0)
            {
                _prioritySelfPosition = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId] + Vector2Int.right;
                return;
            }

            _prioritySelfPosition = null;
        }

        private List<Vector2Int> getEnemiesCloseToBase()
        {
            Vector2Int unitBase = _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
            IEnumerable<IReadOnlyUnit> enemyUnits = _runtimeModel.RoBotUnits;

            int middleX = (int)Math.Round(_runtimeModel.RoMap.Width / 2f);

            return enemyUnits
                .Where((enemy) => enemy.Pos.x < middleX)
                .Select((enemy) => enemy.Pos)
                .OrderBy((enemyPosition) => Vector2Int.Distance(enemyPosition, unitBase))
                .ToList();
        }

        private List<Vector2Int> getEnemiesWithLowHealth()
        {
            IEnumerable<IReadOnlyUnit> enemyUnits = _runtimeModel.RoBotUnits;

            return enemyUnits
                .OrderBy((enemy) => enemy.Health)
                .Select((enemy) => enemy.Pos)
                .ToList();
        }

        public void Dispose()
        {
            _timeUtil.RemoveFixedUpdateAction(getPriorityTargetPosition);
            _timeUtil.RemoveFixedUpdateAction(getPrioritySelfPosition);
        }
    }
}