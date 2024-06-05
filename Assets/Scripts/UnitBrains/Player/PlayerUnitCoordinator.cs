using Model;
using Model.Runtime.ReadOnly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Utilities;

namespace UnitBrains.Player
{
    public class PlayerUnitCoordinator : IDisposable, IUnitCoordinator
    {
        public Vector2Int RecommendedTarget { get; private set; }
        public Vector2Int RecommendedPosition { get; private set; }

        private IReadOnlyRuntimeModel _runtimeModel;
        private TimeUtil _timeUtil;
        private bool _enemyOnPlayerSide;
        private float _attackRange;

        public PlayerUnitCoordinator()
        {
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
            _timeUtil = ServiceLocator.Get<TimeUtil>();

            _timeUtil.AddFixedUpdateAction(UpdatePlayerUnitCoordinator);
        }

        private void UpdatePlayerUnitCoordinator(float deltaTime)
        {
            var botUnits = _runtimeModel.RoBotUnits.ToList();
            if (botUnits.Count == 0)
            {
                var botBasePosition = _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
                RecommendedTarget = botBasePosition;
                RecommendedPosition = botBasePosition;
            }
            else
            {
                CheckPlayerSideForEnemies();
                GetRecommendedTarget(botUnits);
                GetRecommendedPosition(botUnits);
            }
        }
        public void GetRecommendedTarget(List<IReadOnlyUnit> botUnits)
        {
            if (_enemyOnPlayerSide)
                SortByDistanceToPlayerBase(botUnits);
            else SortByHealth(botUnits);
            RecommendedTarget = botUnits.First().Pos;
        }
        public void GetRecommendedPosition(List<IReadOnlyUnit> botUnits)
        {
            if (_enemyOnPlayerSide)
                RecommendedPosition = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId] + Vector2Int.up;
            else
            {
                SortByDistanceToPlayerBase(botUnits);
                //_attackRange = _runtimeModel.RoPlayerUnits.First().Config.AttackRange;
                var playerUnits = _runtimeModel.RoPlayerUnits.ToList();

                if (playerUnits.Count != 0)
                    _attackRange = playerUnits.First().Config.AttackRange;
                else _attackRange = 3.5f;

                var x = botUnits.First().Pos.x;
                var y = botUnits.First().Pos.y - (int)Math.Round(_attackRange);
                RecommendedPosition = new Vector2Int(x, y);
            }
        }
        private void CheckPlayerSideForEnemies()
        {
            _enemyOnPlayerSide = false;
            int playerBaseY = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId].y;
            int botBaseY = _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId].y;
            int distanceToCenter = (botBaseY - playerBaseY) / 2;
            foreach (var unit in _runtimeModel.RoBotUnits)
            {
                if ((unit.Pos.y - playerBaseY) <= distanceToCenter)
                {
                    _enemyOnPlayerSide = true;
                    return;
                }
            }
        }
        private void SortByHealth(List<IReadOnlyUnit> units)
        {
            units.Sort(CompareByHealth);
        }
        private void SortByDistanceToPlayerBase(List<IReadOnlyUnit> units)
        {
            units.Sort(CompareByDistanceToPlayerBase);
        }
        private int CompareByHealth(IReadOnlyUnit unitA, IReadOnlyUnit unitB)
        {
            var healthA = unitA.Health;
            var healthB = unitB.Health;
            return healthA.CompareTo(healthB);
        }
        private int CompareByDistanceToPlayerBase(IReadOnlyUnit unitA, IReadOnlyUnit unitB)
        {
            var playerBaseId = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];
            var distanceA = Vector2Int.Distance(unitA.Pos, playerBaseId);
            var distanceB = Vector2Int.Distance(unitB.Pos, playerBaseId);
            return distanceA.CompareTo(distanceB);
        }
        public void Dispose()
        {
            _timeUtil.RemoveFixedUpdateAction(UpdatePlayerUnitCoordinator);
        }
    }
}