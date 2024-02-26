using System.Collections.Generic;
using ActionGameFramework.Projectiles;
using Model.Runtime.Projectiles;
using Unity.VisualScripting;
using UnityEditor.SearchService;
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
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////    
            int a = GetTemperature();

            if (a >= overheatTemperature)
            {
                return;
            }

            for (int i = 0; i < a; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }

            IncreaseTemperature();
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
            List<Vector2Int> result = GetReachableTargets();
            float min = float.MaxValue;
            Vector2Int bestTarget = Vector2Int.zero;

            foreach (var vector2  in result)
            {
                var distance = DistanceToOwnBase(vector2);

                if (distance < min)
                {
                    min = distance;
                    bestTarget = vector2;
                }
                
            }
            result.Clear();
            result.Add(bestTarget);
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
    }
}