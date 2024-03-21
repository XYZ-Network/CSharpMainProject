using System;
using System.Collections.Generic;
using System.Linq;
using Model.Runtime.ReadOnly;
using UnityEngine;
using Utilities;

namespace Model.Runtime
{
    public class PlayerUnitCoordinator : IDisposable
    {
        private static PlayerUnitCoordinator _instance;
        
        public Vector2Int RecommendedTarget { get; private set; }
        public Vector2Int RecommendedPoint { get; private set; }
        private IReadOnlyRuntimeModel _runtimeModel;
        private TimeUtil _timeUtil;
        private UnitSorter _unitSorter;
        private float _playerAttackRange;
        private bool _enemiesOnPlayerHalf;

        private PlayerUnitCoordinator()
        {
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
            _timeUtil = ServiceLocator.Get<TimeUtil>();
            _unitSorter = new UnitSorter();
            _playerAttackRange = _runtimeModel.RoPlayerUnits.First().Config.AttackRange;
            
            _timeUtil.AddFixedUpdateAction(UpdateRecommendations);
        }

        
        public static PlayerUnitCoordinator GetInstance()
        {
            if (_instance == null)
                _instance = new PlayerUnitCoordinator();
            
            return _instance;
        }

        
        private void UpdateRecommendations(float deltaTime)
        {
            var botUnits = _runtimeModel.RoBotUnits.ToList();
            
            if (botUnits.Any())
            {
                CheckBorder();
                UpdateRecommendedTarget(botUnits);
                UpdateRecommendedPoint(botUnits);
                return;
            }
            
            var botBasePosition = _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
            RecommendedTarget = botBasePosition;
            RecommendedPoint = botBasePosition;
        }
        
        
        private void UpdateRecommendedTarget(List<IReadOnlyUnit> botUnits)
        {
            if (_enemiesOnPlayerHalf)
                _unitSorter.SortByDistanceToBase(botUnits, EBaseType.PlayerBase);
            else
                _unitSorter.SortByHealth(botUnits);
            
            RecommendedTarget = botUnits.First().Pos;
        }
        
        
        public void UpdateRecommendedPoint(List<IReadOnlyUnit> botUnits)
        {
            if (_enemiesOnPlayerHalf)
            {
                RecommendedPoint = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId] + Vector2Int.up;
            }
            else
            {
                _unitSorter.SortByDistanceToBase(botUnits, EBaseType.PlayerBase);
                int x = botUnits.First().Pos.x;
                int y = botUnits.First().Pos.y - Mathf.FloorToInt(_playerAttackRange);
                RecommendedPoint = new Vector2Int(x, y);
            }
        }


        private void CheckBorder()
        {
            foreach (var unit in _runtimeModel.RoBotUnits)
            {
                if (BorderIsCrossed(unit.Pos))
                {
                    _enemiesOnPlayerHalf = true;
                    return;
                }
            }
            
            _enemiesOnPlayerHalf = false;
        }

        
        private bool BorderIsCrossed(Vector2Int botPos)
        {
            int playerBaseY = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId].y;
            int botBaseY = _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId].y;
            int pointsToBorder = (botBaseY - playerBaseY) / 2;

            return botBaseY - botPos.y > pointsToBorder;
        }

        public void Dispose()
        {
            _timeUtil.RemoveFixedUpdateAction(UpdateRecommendations);
        }
    }
}