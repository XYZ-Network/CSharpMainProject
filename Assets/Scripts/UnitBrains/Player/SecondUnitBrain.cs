using System;
using System.Collections.Generic;
using System.Linq;
using Model.Runtime.Projectiles;
using UnityEngine;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Cobra Commando";
        protected override Vector2Int RecommendedPointOffset => Vector2Int.right;

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


            int temperature = GetTemperature();
             if (temperature >= overheatTemperature)
            return;

            IncreaseTemperature();

             int projectilesCount = temperature + 1; 

             for (int i = 0; i < projectilesCount; i++)
            {
             var projectile = CreateProjectile(forTarget);
             AddProjectileToList(projectile, intoList);
            Debug.Log($"Projectile {i + 1}/{projectilesCount} created toward {forTarget} with temperature {temperature}"); 
            } 
        }
            
            ///////////////////////////////////////           

        public override Vector2Int GetNextStep()
        {
            return base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            if (TryGetRecommendedTarget(out var recommendedTarget))
                return new List<Vector2Int> { recommendedTarget };

        ///////////////////////////////////////
        // Homework 1.4 (1st block, 4rd module)
        ///////////////////////////////////////
        List<Vector2Int> allTargets = GetAllTargets().ToList();
        if (allTargets.Count == 0)
            return allTargets;

        float minDistance = float.MaxValue;
        Vector2Int mostDangerousTarget = allTargets[0];

        foreach (Vector2Int target in allTargets)
        {
            float distance = DistanceToOwnBase(target);
            if (distance < minDistance)
            {
                minDistance = distance;
                mostDangerousTarget = target;
            }
        }

        var result = new List<Vector2Int>();
        if (IsTargetInRange(mostDangerousTarget))
            result.Add(mostDangerousTarget);

        return result;
        }
            ///////////////////////////////////////
        

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
