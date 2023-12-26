using System.Collections.Generic;
using Model.Runtime.Projectiles;
using UnityEngine;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Cobra Commando";
        private const float OverheatTemperature = 2f;
        private const float OverheatCooldown = 2f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            var overheatTemperature = OverheatTemperature;
            
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////
            var projectile = CreateProjectile(forTarget);
            AddProjectileToList(projectile, intoList);
            ///////////////////////////////////////
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            var result = GetReachableTargets();
            while (result.Count > 1)
                result.RemoveAt(result.Count - 1);
            return result;
            ///////////////////////////////////////
        }

        public override void Update(float deltaTime, float time)
        {
            if (_temperature > OverheatTemperature)
            {
                if (_cooldownTime <= 0f)
                    _cooldownTime = time + OverheatCooldown;

                if (time > _cooldownTime)
                {
                    _temperature = _cooldownTime = 0f;
                }
            }
            else
            {
                _temperature = Mathf.Max(0f, _temperature - deltaTime);
            }
        }

        private int GetTemperature()
        {
            return (int)_temperature;
        }

        private void IncreaseTemperature()
        {
            _temperature += 0.5f;
        }
    }
}