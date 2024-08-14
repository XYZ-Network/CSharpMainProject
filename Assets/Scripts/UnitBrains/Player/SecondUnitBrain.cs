using System.Collections.Generic;
using Model.Runtime.Projectiles;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

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
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;         

            if(GetTemperature() < overheatTemperature)
            {
                for (float i = -1; i < GetTemperature() ; i++)
                {
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
                                   
                }
            }
            else
            {
                return; 
            }
            IncreaseTemperature();
                                                
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////           
            //var projectile = CreateProjectile(forTarget);
            //AddProjectileToList(projectile, intoList);
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            return base.GetNextStep();
        }
        
        protected override List<Vector2Int> SelectTargets()
        {
           
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            float minDisToEnemy = float.MaxValue;
            Vector2Int nearEnemy = Vector2Int.zero;
           List<Vector2Int> result = GetReachableTargets();
            

                if (result.Count > 0) 
            {
                result.RemoveAt(result.Count - 1);
                    foreach (Vector2Int enemyPos in result)
                    {
                        float disToCurrentEenemy = DistanceToOwnBase(enemyPos);
                        if (minDisToEnemy > disToCurrentEenemy)
                        {
                            minDisToEnemy = disToCurrentEenemy;
                            nearEnemy = enemyPos;
                        }
                    }
                    if (result.Contains(nearEnemy))
                    {
                        result.Clear();
                        result.Add(nearEnemy);
                    }
                    
            }
            return result;
            ///////////////////////////////////////
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