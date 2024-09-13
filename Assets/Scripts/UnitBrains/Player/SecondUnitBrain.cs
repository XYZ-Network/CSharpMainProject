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

        private Vector2Int _notRangeEnemyPosition;

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            //float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////           
            if (!_overheated)
            {
                for (float i = 0; i <= GetTemperature(); i++)
                {
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
                }
                IncreaseTemperature();
            }

            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            return unit.Pos.CalcNextStepTowards(_notRangeEnemyPosition);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            ///
            Vector2Int importantTargetEnemies = new Vector2Int();

            List<Vector2Int> allTargetEnemies = GetAllTargets().ToList();

            Vector2Int targetBaseEnemy = runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];

            //allTargetEnemies.Remove(targetBaseEnemy);

            if (allTargetEnemies.Count > 0)
            {
                if (!HasTargetsInRange())
                {
                    _notRangeEnemyPosition = GetDangerousEnemy(allTargetEnemies);
                }
                else
                {
                    importantTargetEnemies = GetDangerousEnemy(allTargetEnemies);
                }
            }
            else
            {

                if (!HasTargetsInRange())
                {
                    _notRangeEnemyPosition = targetBaseEnemy;

                }
                else
                {
                    importantTargetEnemies = targetBaseEnemy;
                }
            }

            if (!(importantTargetEnemies == Vector2Int.zero))
            {
                allTargetEnemies.Clear();
                allTargetEnemies.Add(importantTargetEnemies);
            }
            else
            {
                allTargetEnemies.Clear();
            }

            return allTargetEnemies;
            ///////////////////////////////////////
        }

        private Vector2Int GetDangerousEnemy(List<Vector2Int> targetEnemies)
        {
            Vector2Int minDistantEnemy = new(int.MaxValue, int.MaxValue);

            foreach (Vector2Int enemy in targetEnemies)
            {
                if (DistanceToOwnBase(minDistantEnemy) < DistanceToOwnBase(enemy))
                {
                    continue;
                }

                minDistantEnemy = enemy;
            }

            return minDistantEnemy;
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
    }
}