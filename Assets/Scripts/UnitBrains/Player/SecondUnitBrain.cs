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
            
            if (GetTemperature() >= overheatTemperature){
                    return;
                }

            for (float i = 0f; i <= GetTemperature(); i++){
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
                // Debug.Log("Текущая температура " + GetTemperature());
                // Debug.Log("Номер снаряда " + (i + 1f));
            }

            IncreaseTemperature();

            ///////////////////////////////////////           
            // var projectile = CreateProjectile(forTarget);
            // AddProjectileToList(projectile, intoList);
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
            /// DistanceToOwnBase - возвращает текущее расстояние от цели до базы

            var minRange = float.MaxValue;
            Vector2Int target = Vector2Int.one;
            float curRange;
        
            List<Vector2Int> result = GetReachableTargets();
            bool isNotEmpty = result.Count > 0;
            while (result.Count > 0){
                curRange = DistanceToOwnBase(result[result.Count - 1]);
                if (curRange < minRange){
                    target = result[result.Count - 1];
                    minRange = curRange;
                }
                result.RemoveAt(result.Count - 1);     
            }

            if(isNotEmpty){
                result.Add(target);
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