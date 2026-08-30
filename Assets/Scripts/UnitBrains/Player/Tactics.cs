using System.Collections.Generic;
using System.Linq;
using Codice.Client.Common.GameUI;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    public class DefaultPlayerUnitTactics
    {
        private static DefaultPlayerUnitTactics _instance;
        private static IReadOnlyRuntimeModel _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
        private static TimeUtil _timeUtil = ServiceLocator.Get<TimeUtil>();
        
        private DefaultPlayerUnitTactics()
        { }
        
        private float DistanceToOwnBase(Vector2Int fromPos, int id = RuntimeModel.PlayerId)
        {
            return Vector2Int.Distance(fromPos, _runtimeModel.RoMap.Bases[id]);
        }

        public PositionWithDanger GetPriorityTarget(bool isPlayerUnitBrain)
        {
            float distanceBetweenBases = Vector2Int.Distance(_runtimeModel.RoMap.Bases[0], _runtimeModel.RoMap.Bases[1]);
            if (isPlayerUnitBrain)
            {
                if (_runtimeModel.RoBotUnits.Any())
                {
                    if (_runtimeModel.RoBotUnits.Any(u => DistanceToOwnBase(u.Pos) < distanceBetweenBases / 2))
                    {
                        return new PositionWithDanger(_runtimeModel.RoBotUnits.OrderBy(u => DistanceToOwnBase(u.Pos)).First().Pos, 1);
                    }
                    return new PositionWithDanger(_runtimeModel.RoBotUnits.OrderBy(u => u.Health).First().Pos, 0);

                }
                else
                {
                    return new PositionWithDanger(_runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId], 1);
                }
            }
            else
            {
                if (_runtimeModel.RoPlayerUnits.Any())
                {
                    if (_runtimeModel.RoPlayerUnits.Any(u => DistanceToOwnBase(u.Pos, RuntimeModel.BotPlayerId) < distanceBetweenBases / 2))
                    {
                        return new PositionWithDanger(_runtimeModel.RoPlayerUnits.OrderBy(u => DistanceToOwnBase(u.Pos, RuntimeModel.BotPlayerId)).First().Pos, 1);
                    }
                    return new PositionWithDanger(_runtimeModel.RoPlayerUnits.OrderBy(u => u.Health).First().Pos, 0);

                }
                else
                {
                    return new PositionWithDanger(_runtimeModel.RoMap.Bases[RuntimeModel.PlayerId], 1);
                }

            }
        }

        public static DefaultPlayerUnitTactics GetInstance()
        {
            if (_instance == null)
            {
                _instance = new DefaultPlayerUnitTactics();
            }
            return _instance;
        }
    }
}