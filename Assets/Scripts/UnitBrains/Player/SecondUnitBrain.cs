using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;
using Utilities;
using static UnityEngine.GraphicsBuffer;

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

        public List<Vector2Int> NoReachableTargets = new List<Vector2Int>();
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            int temp = GetTemperature();
            if (temp < overheatTemperature)
            {
                IncreaseTemperature();
            }   
            else
            {
                return;
            }

            
            for (int i = 0; i <= temp; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
            
            
        }

        public override Vector2Int GetNextStep()
        {
            Vector2Int target = Vector2Int.zero;
            if (NoReachableTargets.Any())
                target = NoReachableTargets[0];
            else
                target = unit.Pos;

            if (IsTargetInRange(target))
                return unit.Pos;
            else
                return unit.Pos.CalcNextStepTowards(target);
        }

        protected override List<Vector2Int> SelectTargets()
        {

            List<Vector2Int> resultList = new List<Vector2Int>();
            Vector2Int target = GetNearesTarget(GetAllTargets().ToList());
            NoReachableTargets.Clear();

            if (target.magnitude != 0)
            {
                NoReachableTargets.Add(target);
                if (IsTargetInRange(target))
                    resultList.Add(target);
            }
            else
            {
                resultList.Add(runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]); ;
            }
            return resultList;
        }

        private Vector2Int GetNearesTarget(List<Vector2Int> enemyUnits)
        {
            Vector2Int nearestPositionOfEnemy = Vector2Int.zero;
            float minDistanceToEnemy = float.MaxValue;

                foreach (Vector2Int enemyPosition in enemyUnits)
                {
                    float distanceToCurrentEnemy = DistanceToOwnBase(enemyPosition);
                    if (distanceToCurrentEnemy < minDistanceToEnemy)
                    {
                        minDistanceToEnemy = distanceToCurrentEnemy;
                        nearestPositionOfEnemy = enemyPosition;
                    }
    ;
                }
            if (minDistanceToEnemy < float.MaxValue)
                return nearestPositionOfEnemy;
            else
                return Vector2Int.zero;
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