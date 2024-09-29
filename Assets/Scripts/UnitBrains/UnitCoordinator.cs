using Model;
using Model.Runtime;
using Model.Runtime.ReadOnly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitBrains;
using UnityEngine;
using Utilities;

namespace Assets.Scripts.UnitBrains
{
    public class UnitCoordinator
    {
        private IReadOnlyRuntimeModel _runtimeModel => ServiceLocator.Get<IReadOnlyRuntimeModel>();
        private TimeUtil _timeUtil => ServiceLocator.Get<TimeUtil>();

        public Vector2Int? GetTarget(BaseUnitBrain unitBrain) 
        {
            List <Vector2Int> enemiesCloseToBase = getEnemiesCloseToBase(unitBrain);
            if (enemiesCloseToBase.Count > 0) return enemiesCloseToBase[0];

            List <Vector2Int> enemiesWithLowHealth = getEnemiesWithLowHealth(unitBrain);
            if (enemiesWithLowHealth.Count > 0) return enemiesWithLowHealth[0];

            return null;
        }

        public Vector2Int? GetPosition(BaseUnitBrain unitBrain)
        {
            List<Vector2Int> enemiesCloseToBase = getEnemiesCloseToBase(unitBrain);

            if (enemiesCloseToBase.Count > 0)
            {
                return unitBrain.IsPlayerUnitBrain
                ? _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId] + Vector2Int.right
                : _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId] + Vector2Int.left;
            }

            return null;
        }

        /**
         * Units that are located on half of the map next to the base are considered close to the base.
         */
        private List<Vector2Int> getEnemiesCloseToBase(BaseUnitBrain unitBrain)
        {
            Vector2Int unitBase = _runtimeModel.RoMap.Bases[unitBrain.IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
            IEnumerable<IReadOnlyUnit> enemyUnits = unitBrain.IsPlayerUnitBrain
                ? _runtimeModel.RoBotUnits
                : _runtimeModel.RoPlayerUnits;

            int middleX = (int)Math.Round(_runtimeModel.RoMap.Width / 2f);

            return enemyUnits
                .Where((enemy) => unitBrain.IsPlayerUnitBrain ? enemy.Pos.x < middleX : enemy.Pos.x > middleX)
                .Select((enemy) => enemy.Pos)
                .OrderBy((enemyPosition) => Vector2Int.Distance(enemyPosition, unitBase))
                .ToList();
        }

        private List<Vector2Int> getEnemiesWithLowHealth(BaseUnitBrain unitBrain)
        {
            IEnumerable<IReadOnlyUnit> enemyUnits = unitBrain.IsPlayerUnitBrain
                ? _runtimeModel.RoBotUnits
                : _runtimeModel.RoPlayerUnits;

            return enemyUnits
                .OrderBy((enemy) => enemy.Health)
                .Select((enemy) => enemy.Pos)
                .ToList();
        }
    }
}
