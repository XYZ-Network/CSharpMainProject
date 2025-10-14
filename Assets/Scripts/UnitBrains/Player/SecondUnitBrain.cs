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
                
           
            if (_temperature >= overheatTemperature)           //проверка на перегрев
                return;

            IncreaseTemperature();                            //повышение температуры

            _temperature = GetTemperature();                 //узнаем температуру

            for (int i = 0; i < _temperature; i++)          //цикл для создания снарядов равной температуре

            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
;           

        }


        public override Vector2Int GetNextStep()
        {
            return base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            
            List<Vector2Int> result = GetReachableTargets();

            
            Vector2Int closetTarget = result[0];
            float minDistance = DistanceToOwnBase(closetTarget);

            foreach (Vector2Int target in result)
            {
                if (DistanceToOwnBase(target) < minDistance)
                {
                    minDistance = DistanceToOwnBase(target);
                    closetTarget = target;
                }
            }
            while (result.Count > 1)
            {
                result.RemoveAt(result.Count - 1);
            }
            if (result.Count > 0)
            {
                result[0] = closetTarget;
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
    }
}