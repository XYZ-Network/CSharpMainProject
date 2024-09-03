using Model;
using Model.Config;
using Model.Runtime.ReadOnly;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Utilities;


    public class UnitCoordinator
    {
        private readonly TimeUtil _timeUtil = ServiceLocator.Get<TimeUtil>();
        protected static IReadOnlyRuntimeModel runtimeModel => ServiceLocator.Get<IReadOnlyRuntimeModel>();
        private bool _isPlayerUnitBrain = true;
        private int _mapLenght;
        private Vector2Int target;
        private Vector2Int targetPos;
        private Vector2Int basePoint;


        public UnitCoordinator(bool _isPlayerUnitBrain)
        {
        this._isPlayerUnitBrain = _isPlayerUnitBrain;
        _timeUtil.AddFixedUpdateAction(UpdateEnemies);
        }


        public Vector2Int GetTargetRecommendation()
        {
            return target;
        }
        public Vector2Int GetTargetPosRecommendation()
        {
            return targetPos;
        }

        private void GetEnemies()
        {
        basePoint = runtimeModel.RoMap.Bases[
                    _isPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
            List<IReadOnlyUnit> enemyNearBase = new List<IReadOnlyUnit>();
        foreach (var enemy in _isPlayerUnitBrain? runtimeModel.RoBotUnits : runtimeModel.RoPlayerUnits)
        {
            if (Vector2Int.Distance(basePoint,enemy.Pos) <= 5)
            {
                enemyNearBase.Add(enemy);
            }
        }

        if (enemyNearBase.Count > 0)
            {
                Vector2Int targetEnemy = enemyNearBase[0].Pos;
                float minDistance = Vector2Int.Distance(targetEnemy, basePoint);
                for (int i = 1; i < enemyNearBase.Count; i++)
                {
                    float distance = Vector2Int.Distance(enemyNearBase[i].Pos, basePoint);
                    if (distance < minDistance)
                    {
                        targetEnemy = enemyNearBase[i].Pos;
                        minDistance = distance;
                    };
                }
                targetPos = new Vector2Int(basePoint.x + (_isPlayerUnitBrain ? -1 : 1), basePoint.y); //Рекомендуемая точка.
                target = targetEnemy; // Устанавливаем Рекомендуемую цель.
            }
          else if (_isPlayerUnitBrain ? runtimeModel.RoBotUnits.Count() > 0 : runtimeModel.RoPlayerUnits.Count() > 0)
            {
                int MinHealth = int.MaxValue;
                float minDistance = float.MaxValue;
                foreach (var enemy in _isPlayerUnitBrain ? runtimeModel.RoBotUnits : runtimeModel.RoPlayerUnits)
            {
                    float distance = Vector2Int.Distance(enemy.Pos, basePoint);
                    if (distance < minDistance)
                    {

                        minDistance = distance;
                        targetPos = new Vector2Int(enemy.Pos.x + (int)(_isPlayerUnitBrain ? -enemy.Config.AttackRange : enemy.Config.AttackRange),
                            enemy.Pos.y); //рекомендуемая точка находится на расстоянии выстрела от ближайшего к базе врага.
                    }
                    if (enemy.Health < MinHealth)
                    {
                        targetPos = enemy.Pos;
                        MinHealth = enemy.Health;
                    }
                }
            }
           else
        {
            targetPos = basePoint;
        }
        }

        public void UpdateEnemies(float deltaTime)
        {

        if (runtimeModel.Stage != RuntimeModel.GameStage.None) 
            GetEnemies();

        }
        public void GetRecommendationDelayed(Vector2Int unitPos, int playerId, float delay = 1f)
        {
            _timeUtil.RunDelayed(delay, () => GetEnemies());
        }


    }
