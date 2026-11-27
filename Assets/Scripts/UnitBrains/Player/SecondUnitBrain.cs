using System.Collections.Generic;
using Model.Runtime.Projectiles;
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
            if (GetTemperature() >= overheatTemperature)
                return;

            // I think GetTemperature() function is redundant, I'd use
            //if (_overheated)
            //    return;
            // 'cos it's already calculated in IncreaseTemperature() method

            // float overheatTemperature = OverheatTemperature; is also redundant

            IncreaseTemperature();
            for (int i = 0; i < _temperature; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            return base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4th module)
            ///////////////////////////////////////
            List<Vector2Int> result = GetReachableTargets();
            Vector2Int maxTargetDistance = new Vector2Int(int.MaxValue, int.MaxValue);
            Vector2Int closestTarget = maxTargetDistance;

            foreach (Vector2Int target in result)
                if (DistanceToOwnBase(target) < DistanceToOwnBase(closestTarget))
                    closestTarget = target;

            if (closestTarget != maxTargetDistance)
            {
                result.Clear();
                result.Add(closestTarget);
            }
            else
                while (result.Count > 1)
                    result.RemoveAt(result.Count - 1);

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