using Model;
using Model.Runtime.ReadOnly;
using System;
using System.Collections.Generic;
using System.Linq;
using UnitBrains;
using UnityEngine;
using Utilities;

namespace Assets.Scripts.UnitBrains
{
    public class UnitCoordinator
    {
        private readonly IReadOnlyRuntimeModel _runtimeModel;
        private readonly TimeUtil _timeUtil;

        public UnitCoordinator(IReadOnlyRuntimeModel readOnlyRuntimeModel, TimeUtil timeUtil)
        {
            _runtimeModel = readOnlyRuntimeModel;
            _timeUtil = timeUtil;
        }

        public Vector2Int? GetRecomendedTargetFor(BaseUnitBrain unitBrain)
        {
            var midPointX = _runtimeModel.RoMap.Width / 2;

            Func<IReadOnlyUnit, bool> closeToBaseExpression = unitBrain.IsPlayerUnitBrain
                ? bu => bu.Pos.x < midPointX
                : pu => pu.Pos.x > midPointX;

            Vector2Int currentBase = unitBrain.IsPlayerUnitBrain
                ? _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]
                : _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];

            IEnumerable<IReadOnlyUnit> currentEnemyUnits = unitBrain.IsPlayerUnitBrain
                ? _runtimeModel.RoBotUnits
                : _runtimeModel.RoPlayerUnits;

            var unitsCloseToBaseBase = currentEnemyUnits
                .Where(closeToBaseExpression);

            if (unitsCloseToBaseBase.Any())
            {
                return unitsCloseToBaseBase
                    .OrderBy(u => Vector2Int.Distance(u.Pos, currentBase))
                    .FirstOrDefault()
                    ?.Pos;
            }

            return currentEnemyUnits.OrderBy(u => u.Health).FirstOrDefault()?.Pos;
        }

        public Vector2Int? GetRecomendedLocation(BaseUnitBrain unitBrain)
        {
            var midPointX = _runtimeModel.RoMap.Width / 2;

            Func<IReadOnlyUnit, bool> closeToBaseExpression = unitBrain.IsPlayerUnitBrain
                ? bu => bu.Pos.x < midPointX
                : pu => pu.Pos.x > midPointX;

            Vector2Int currentBase = unitBrain.IsPlayerUnitBrain
                ? _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]
                : _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];

            IEnumerable<IReadOnlyUnit> currentEnemyUnits = unitBrain.IsPlayerUnitBrain
                ? _runtimeModel.RoBotUnits
                : _runtimeModel.RoPlayerUnits;

            if (currentEnemyUnits
                .Where(closeToBaseExpression)
                .Any())
            {
                if (unitBrain.IsPlayerUnitBrain)
                {
                    return currentBase + Vector2Int.right;
                }
                else
                {
                    return currentBase + Vector2Int.left;
                }
            }

            return currentEnemyUnits
                .OrderBy(eu => eu.Health)
                .FirstOrDefault()?.Pos;
        }
    }
}