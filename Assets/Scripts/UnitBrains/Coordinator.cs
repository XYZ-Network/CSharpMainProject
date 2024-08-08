using Model;
using Model.Runtime.ReadOnly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Utilities;

namespace Assets.Scripts.UnitBrains
{

    internal struct UnitDistance
    {
        public double Distance;
        public IReadOnlyUnit Unit;

        public UnitDistance(double distance, IReadOnlyUnit unit)
        {
            this.Distance = distance;
            this.Unit = unit;
        }
    }

    public class Coordinator
    {
        private readonly int PlayerID;
        private readonly double midpointDistance = 0;
        private readonly Vector2Int playerBase;
        private readonly Vector2Int botPlayerBase;

        public Coordinator(int forPlayer) {
            PlayerID = forPlayer;
            playerBase = runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];
            botPlayerBase = runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
            var midpoint = new Vector2Int((playerBase.x + botPlayerBase.x) / 2, (playerBase.y + botPlayerBase.y) / 2);
            midpointDistance = Math.Sqrt(Math.Pow(playerBase.x - midpoint.x, 2) + Math.Pow(playerBase.y - midpoint.y, 2));
            timeUtil.AddFixedUpdateAction(FixedUpdate);
        }

        public IReadOnlyUnit? recommendedUnit;
        public Vector2Int? recommendedPos;

        protected IReadOnlyRuntimeModel runtimeModel => ServiceLocator.Get<IReadOnlyRuntimeModel>();
        protected TimeUtil timeUtil => ServiceLocator.Get<TimeUtil>();

        private void FixedUpdate(float timeDelta)
        {
            var enemiesOnTerritory = runtimeModel.RoUnits
                .Select(unit => new UnitDistance(distanceToBase(unit), unit))
                .Where(unitDistance => unitDistance.Distance <= midpointDistance)
                .ToList();
            if (enemiesOnTerritory.Any())
            {
                //if enemy is on our territory, select closest one to our base
                recommendedUnit = enemiesOnTerritory
                    .Where(unitDistance => PlayerID == RuntimeModel.PlayerId 
                            ? !unitDistance.Unit.Config.IsPlayerUnit 
                            : unitDistance.Unit.Config.IsPlayerUnit)
                    .OrderBy(unitDistance => unitDistance.Distance)
                    .Select(unitDistance => unitDistance.Unit)
                    .FirstOrDefault();

                recommendedPos = PlayerID == RuntimeModel.PlayerId
                    ? new Vector2Int(playerBase.x + 1, playerBase.y)
                    : new Vector2Int(botPlayerBase.x - 1, botPlayerBase.y);
            }
            else
            {
                //else select one with lowest health value
                recommendedUnit = runtimeModel.RoUnits
                    .Where(unit => PlayerID == RuntimeModel.PlayerId
                                ? !unit.Config.IsPlayerUnit
                                : unit.Config.IsPlayerUnit)
                    .OrderBy(unit => unit.Health)
                    .FirstOrDefault();

                var closestUnit = runtimeModel.RoUnits
                    .Where(unit => PlayerID == RuntimeModel.PlayerId
                                    ? unit.Config.IsPlayerUnit
                                    : !unit.Config.IsPlayerUnit)
                    .OrderBy(unit => distanceToBase(unit))
                    .FirstOrDefault();

                recommendedPos = closestUnit?.Pos;
            }
        }

        public void Update()
        {
        }

        private double distanceToBase(IReadOnlyUnit unit)
        {
            var opponentBase = runtimeModel.RoMap.Bases[unit.Config.IsPlayerUnit ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
            var distance = Math.Sqrt(Math.Pow(opponentBase.x - unit.Pos.x, 2) + Math.Pow(opponentBase.y - unit.Pos.y, 2));
            return distance;
        }

        //private bool IsEnemyOnOwnHalf(IReadOnlyUnit unit)
        //{
        //    var ownBase = runtimeModel.RoMap.Bases[unit.Config.IsPlayerUnit ? RuntimeModel.PlayerId : RuntimeModel.BotPlayerId];
        //    var enemyBase = runtimeModel.RoMap.Bases[unit.Config.IsPlayerUnit ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
        //    var midpoint = new Vector2Int((ownBase.x + enemyBase.x) / 2, (ownBase.y + enemyBase.y) / 2);
        //    var midpointToBase = (midpoint - ownBase).sqrMagnitude;
        //    var distance = unit.Pos - ownBase;
        //    return distance.sqrMagnitude <= midpointToBase;
        //}

    }
}
