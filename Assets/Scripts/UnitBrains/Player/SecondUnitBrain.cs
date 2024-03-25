using System.Collections.Generic;
using Model.Runtime.Projectiles;
using Unity.VisualScripting;
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
            
                for (GetTemperature(); GetTemperature() < overheatTemperature; IncreaseTemperature()) {
                    if (GetTemperature() == 1)
                    {
                        var projectile = CreateProjectile(forTarget);
                        AddProjectileToList(projectile, intoList);
                     
                    }
                    if (GetTemperature() == 2)
                    {
                        var projectile1 = CreateProjectile(forTarget);
                        AddProjectileToList(projectile1, intoList);
                        var projectile2 = CreateProjectile(forTarget);
                        AddProjectileToList(projectile2, intoList);
                  
                    }
                    if (GetTemperature() == 3)
                    {
                        var projectile3 = CreateProjectile(forTarget);
                        AddProjectileToList(projectile3, intoList);
                        var projectile4 = CreateProjectile(forTarget);
                        AddProjectileToList(projectile4, intoList);
                        var projectile5 = CreateProjectile(forTarget);
                        AddProjectileToList(projectile5, intoList);
                      
                    }
               
                }



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
            List<Vector2Int> result = GetReachableTargets();
            while (result.Count > 1)
            {
                result.RemoveAt(result.Count - 1);
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