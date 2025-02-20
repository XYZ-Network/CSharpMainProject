using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Cobra Commando";
        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;
        private IEnumerable<Vector2Int> _allTargets;
        private List<Vector2Int> _targetInRangeResult = new();
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            var currentTemperature = GetTemperature();

            if (currentTemperature >= overheatTemperature)
            {
                return;
            }

            do
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            } while (intoList.Count < currentTemperature);

            
            IncreaseTemperature();
        }

        public override Vector2Int GetNextStep()
        {
            if (!_targetInRangeResult.Any())
            {
                return unit.Pos;;
            }
            
            return  unit.Pos.CalcNextStepTowards(_targetInRangeResult.First());
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = new();
            _allTargets = GetAllTargets();
            _targetInRangeResult.Clear();

            if (_allTargets.Any())
            {
                var closestEnemyToBase = GetClosestEnemyToBase(_allTargets);

                if (IsTargetInRange(closestEnemyToBase))
                {
                    result.Add(closestEnemyToBase);
                }
                else
                {
                    _targetInRangeResult.Add(closestEnemyToBase);
                }
            }
            else
            {
                result.Add(runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId]);
            }
          
            return result;
        }

        public override void Update(float deltaTime, float time)
        {
            if (_overheated)
            {              
                _cooldownTime += Time.deltaTime;
                float t = _cooldownTime / (OverheatCooldown/10);
                _temperature = Mathf.Lerp(OverheatTemperature, 0, t);
                if (t >= 1)
                {
                    _cooldownTime = 0;
                    _overheated = false;
                }
            }
        }
        
        private int GetTemperature()
        {
            if(_overheated) return (int) OverheatTemperature;
            else return (int)_temperature;
        }

        private void IncreaseTemperature()
        {
            _temperature += 1f;
            if (_temperature >= OverheatTemperature) _overheated = true;
        }

        private Vector2Int GetClosestEnemyToBase(IEnumerable<Vector2Int> allTargets)
        {
            float minDistance = float.MaxValue;
            Vector2Int resultClosestEnemy = new Vector2Int();
            
            foreach (Vector2Int target in allTargets) 
            {
                float targetDistance = DistanceToOwnBase(target);

                if (targetDistance < minDistance) 
                {
                    minDistance = targetDistance;
                    resultClosestEnemy = target;
                }
            }
            
            return resultClosestEnemy;
        }
    }
}