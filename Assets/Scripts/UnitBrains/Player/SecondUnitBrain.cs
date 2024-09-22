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
            
        if (GetTemperature() >= overheatTemperature)
        //Проверка текущей температуры на перегрев
        {
                return;
                //Завершение стрельбы если идет перегрев
        }
        //Увеличение снарядов с каждым выстрелом
        for (float i = 0f; i <= GetTemperature(); i++)
        {
            var projectile = CreateProjectile(forTarget);
            AddProjectileToList(projectile, intoList);
            
            Debug.Log("Текущая температура" + GetTemperature());
            Debug.Log("Номер снаряда" + (i + 1f));
        }
        IncreaseTemperature();
        }

        public override Vector2Int GetNextStep()
        {
            return base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = GetReachableTargets();
            
            if (result.Count == 0)
            {
                return new List<Vector2Int>();
            }

            Vector2Int targetEnemy = new Vector2Int();

            float minDistance = float.MaxValue;

            foreach (Vector2Int enemy in result)
            {
                float spacing = DistanceToOwnBase(enemy);
                if (spacing < minDistance)
                {
                    minDistance = spacing;
                    targetEnemy = enemy;
                }
            }

            result.Clear();
            result.Add(targetEnemy);
            
            while (result.Count > 1)
            {
                result.RemoveAt(result.Count - 1);
            }
            return result;
            
            float distance = DistanceToOwnBase(targetEnemy);
            Debug.Log(distance);

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
