using Model;
using Model.Runtime.ReadOnly;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;

namespace Assets.Scripts.UnitBrains.Player
{
    public class UnitsCoordinator
    {
        private static UnitsCoordinator _instance;

        private IReadOnlyRuntimeModel _runtimeModel;
        private TimeUtil _timeUtil;
        private float _attackRange;
        private bool _isEnemyOnPlayerSide = false;
        public Vector2Int recommendedTarget;
        public Vector2Int recommendedPoint;

        private UnitsCoordinator()
        {
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
            _timeUtil = ServiceLocator.Get<TimeUtil>();
            _attackRange = _runtimeModel.RoPlayerUnits.First().Config.AttackRange;
            _timeUtil.AddUpdateAction(UpdateRecommendations);
        }

        public static UnitsCoordinator GetInstance()
        {
            if (_instance == null)
                _instance = new UnitsCoordinator();

            return _instance;
        }

        private void UpdateRecommendations(float deltaTime)
        {
            List<IReadOnlyUnit> targetList = _runtimeModel.RoBotUnits.ToList();

            if (targetList.Count == 0)
            {
                Vector2Int botBasePos = _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
                recommendedTarget = botBasePos;
                recommendedPoint = botBasePos;
            }
            else
            {
                IsEnemyOnPlayerSide(targetList);
                SelectRecommendedTarget(targetList);
                SelectRecommendedPoint(targetList);
            }
        }

        public void SelectRecommendedTarget(List<IReadOnlyUnit> targetList)
        {
            if (_isEnemyOnPlayerSide)
            {
                SortByDistanceToPlayerBase(targetList);
            }
            else
            {
                SortByHealth(targetList);
            }
            if (targetList.Count > 0)
            {
                recommendedTarget = targetList.First().Pos;
            }
            else
            {
                recommendedTarget = _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
            }
        }

        public void SelectRecommendedPoint(List<IReadOnlyUnit> targetList)
        {
            if (_isEnemyOnPlayerSide)
            {
                recommendedPoint = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId] + new Vector2Int(1, 1);
                Debug.Log("RecommendedPoint: " + recommendedPoint);
            }
            else
            {
                SortByDistanceToPlayerBase(targetList);

                int x = targetList.First().Pos.x;
                int y = targetList.First().Pos.y - (int)Math.Round(_attackRange);

                recommendedPoint = new Vector2Int(x, y);
            }
        }

        private void IsEnemyOnPlayerSide(List<IReadOnlyUnit> targetList)
        {
            _isEnemyOnPlayerSide = false;
            int playerSideY = _runtimeModel.RoMap.Height / 2;

            foreach (var target in targetList)
            {
                if (target.Pos.y < playerSideY)
                {
                    _isEnemyOnPlayerSide = true;
                    return;
                }
            }
        }

        private void SortByDistanceToPlayerBase(List<IReadOnlyUnit> targetList)
        {
            targetList.Sort(CompareByDistanceToPlayerBase);
        }

        private void SortByHealth(List<IReadOnlyUnit> targetList)
        {
            targetList.Sort(CompareByHealth);
        }

        private int CompareByDistanceToPlayerBase(IReadOnlyUnit targetA, IReadOnlyUnit targetB)
        {
            var playerBaseId = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];
            var distanceA = Vector2Int.Distance(targetA.Pos, playerBaseId);
            var distanceB = Vector2Int.Distance(targetB.Pos, playerBaseId);
            return distanceA.CompareTo(distanceB);
        }

        private int CompareByHealth(IReadOnlyUnit targetA, IReadOnlyUnit targetB)
        {
            var healthA = targetA.Health;
            var healthB = targetB.Health;
            return healthA.CompareTo(healthB);
        }
    }
}